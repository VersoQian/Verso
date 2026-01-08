using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Managers;
using Game.Modules;
using Game.Pathfinding;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.Enemy.States
{
    /// <summary>
    /// 敌人智能追踪状态 - 基于A*寻路
    /// 混合全局路径规划和局部动态避障
    /// </summary>
    public sealed class EnemySmartFollowState : EnemyState
    {
        [Inject] private Timer _timer;
        [Inject] private GameManager _gameManager;
        [Inject] private LevelManager _levelManager;

        private float _walkSpeed;
        private bool _isPause;
        private TeamIDType _teamID;

        // A*寻路相关
        private PathfindingGrid _pathfindingGrid;
        private AStarPathfinder _pathfinder;
        private List<Vector3> _currentPath;
        private int _currentWaypointIndex;

        // 路径规划参数
        private const float PATH_REPLAN_INTERVAL = 0.5f; // 每0.5秒重新规划路径
        private const float WAYPOINT_REACHED_DISTANCE = 0.5f; // 到达路径点的距离阈值
        private float _lastReplanTime;

        // 平滑转向
        private const float TURN_SMOOTHNESS = 8f;
        private Vector3 _currentDirection;

        // 局部避障(避开其他敌人)
        private const float LOCAL_AVOIDANCE_RADIUS = 2f;
        private const float LOCAL_AVOIDANCE_WEIGHT = 0.3f;
        private LayerMask _enemyLayer;

        public override void Initialize()
        {
            _walkSpeed = _enemy.Model.Speed;
            _teamID = _enemy.TeamID;
            _currentDirection = _enemy.View.RotateNode.forward;

            // 自动获取或创建寻路网格
            _pathfindingGrid = PathfindingManager.Instance.Grid;

            if (_pathfindingGrid == null || !_pathfindingGrid.IsInitialized)
            {
                Debug.LogError("[EnemySmartFollowState] PathfindingGrid初始化失败! 请检查场景配置");
                // 降级到简单移动模式
                return;
            }

            _pathfinder = new AStarPathfinder(_pathfindingGrid);

            // 立即规划第一次路径
            ReplanPath();

            _enemy.View.Walk();

            _levelManager.ON_PAUSE += OnPause;
            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _levelManager.ON_PAUSE -= OnPause;
            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            if (_isPause || _pathfindingGrid == null)
                return;

            // 定期重新规划路径(应对玩家移动)
            if (_timer.Time - _lastReplanTime > PATH_REPLAN_INTERVAL)
            {
                ReplanPath();
            }

            // 如果有有效路径,沿路径移动
            if (_currentPath != null && _currentPath.Count > 0)
            {
                FollowPath();
            }
            else
            {
                // 没有路径,尝试直接朝向玩家(备用方案)
                MoveDirectlyToPlayer();
            }

            CheckCollisionBullets();
            CheckCollisionPlayer();
        }

        /// <summary>
        /// 重新规划路径
        /// </summary>
        private void ReplanPath()
        {
            _lastReplanTime = _timer.Time;

            Vector3 startPos = _enemy.View.Position;
            Vector3 targetPos = _gameManager.Player.View.Position;

            // 使用A*寻找路径
            _currentPath = _pathfinder.FindPath(startPos, targetPos, smoothPath: true);

            if (_currentPath != null && _currentPath.Count > 0)
            {
                _currentWaypointIndex = 0;

                #if UNITY_EDITOR
                // 调试可视化路径
                for (int i = 0; i < _currentPath.Count - 1; i++)
                {
                    Debug.DrawLine(_currentPath[i] + Vector3.up, _currentPath[i + 1] + Vector3.up, Color.cyan, PATH_REPLAN_INTERVAL);
                }
                #endif
            }
        }

        /// <summary>
        /// 沿路径移动
        /// </summary>
        private void FollowPath()
        {
            if (_currentWaypointIndex >= _currentPath.Count)
            {
                // 已到达路径终点
                return;
            }

            Vector3 targetWaypoint = _currentPath[_currentWaypointIndex];
            Vector3 toWaypoint = (targetWaypoint - _enemy.View.Position);
            toWaypoint.y = 0f;

            float distanceToWaypoint = toWaypoint.magnitude;

            // 检查是否到达当前路径点
            if (distanceToWaypoint < WAYPOINT_REACHED_DISTANCE)
            {
                _currentWaypointIndex++;
                if (_currentWaypointIndex >= _currentPath.Count)
                {
                    return;
                }
                targetWaypoint = _currentPath[_currentWaypointIndex];
                toWaypoint = (targetWaypoint - _enemy.View.Position);
                toWaypoint.y = 0f;
            }

            // 计算移动方向
            Vector3 pathDirection = toWaypoint.normalized;

            // 混合局部避障(避开其他敌人)
            Vector3 avoidanceVector = CalculateLocalAvoidance();
            Vector3 finalDirection = (pathDirection * (1f - LOCAL_AVOIDANCE_WEIGHT) + avoidanceVector * LOCAL_AVOIDANCE_WEIGHT).normalized;

            // 平滑转向
            _currentDirection = Vector3.Slerp(_currentDirection, finalDirection, TURN_SMOOTHNESS * Time.deltaTime);
            _currentDirection.y = 0f;
            _currentDirection.Normalize();

            // 应用移动和旋转
            if (_currentDirection != Vector3.zero)
            {
                _enemy.View.Rotation = Quaternion.LookRotation(_currentDirection);
                _enemy.View.Position += _walkSpeed * Time.deltaTime * _currentDirection;
            }
        }

        /// <summary>
        /// 直接朝向玩家移动(备用方案,无路径时使用)
        /// </summary>
        private void MoveDirectlyToPlayer()
        {
            var toPlayer = (_gameManager.Player.View.Position - _enemy.View.Position).normalized;
            toPlayer.y = 0f;

            Vector3 avoidanceVector = CalculateLocalAvoidance();
            Vector3 finalDirection = (toPlayer * 0.7f + avoidanceVector * 0.3f).normalized;

            _currentDirection = Vector3.Slerp(_currentDirection, finalDirection, TURN_SMOOTHNESS * Time.deltaTime);
            _currentDirection.y = 0f;
            _currentDirection.Normalize();

            if (_currentDirection != Vector3.zero)
            {
                _enemy.View.Rotation = Quaternion.LookRotation(_currentDirection);
                _enemy.View.Position += _walkSpeed * Time.deltaTime * _currentDirection;
            }
        }

        /// <summary>
        /// 计算局部避障向量(避开附近的敌人)
        /// </summary>
        private Vector3 CalculateLocalAvoidance()
        {
            Vector3 avoidanceForce = Vector3.zero;
            int neighborCount = 0;

            // 查找附近的其他敌人
            var nearbyEnemies = _gameManager.Enemies
                .Where(e => e != _enemy && Vector3.Distance(e.View.Position, _enemy.View.Position) < LOCAL_AVOIDANCE_RADIUS)
                .ToList();

            foreach (var otherEnemy in nearbyEnemies)
            {
                Vector3 awayFromOther = (_enemy.View.Position - otherEnemy.View.Position);
                float distance = awayFromOther.magnitude;

                if (distance > 0.01f)
                {
                    // 距离越近,排斥力越强
                    float strength = 1f - (distance / LOCAL_AVOIDANCE_RADIUS);
                    avoidanceForce += awayFromOther.normalized * strength;
                    neighborCount++;
                }
            }

            if (neighborCount > 0)
            {
                avoidanceForce /= neighborCount;
                avoidanceForce.y = 0f;
                avoidanceForce = avoidanceForce.normalized;
            }

            return avoidanceForce;
        }

        private void OnPause(bool value)
        {
            _isPause = value;
        }

        private void CheckCollisionBullets()
        {
            foreach (var bullet in _gameManager.Bullets.ToList())
            {
                var type = bullet.Model.Type;
                if (type == Config.BulletControllerType.Parabola)
                    continue;

                var bulletTeamID = bullet.Model.TeamID;
                if (bulletTeamID == _teamID)
                    continue;

                var currentTime = _timer.Time;
                var canCollideUnit = bullet.CanCollideUnit(_enemy, currentTime);
                if (!canCollideUnit)
                    continue;

                var aimPosition = _enemy.View.AimPosition;
                var radius = _enemy.View.Radius;

                var bulletPosition = bullet.View.Position;
                var bulletRadius = bullet.View.Radius;

                var isCollided = VectorExtensions.IsCollided(aimPosition, radius, bulletPosition, bulletRadius);
                if (!isCollided)
                    continue;

                bullet.AddUnit(_enemy, currentTime);
                bullet.FireCollideEnemy();

                var damage = bullet.Model.Damage;
                var direction = (bulletPosition - aimPosition).normalized;
                _enemy.TryToDamage(damage, direction);
            }
        }

        private void CheckCollisionPlayer()
        {
            var enemyAimPosition = _enemy.View.AimPosition;
            var enemyRadius = _enemy.View.Radius;

            var playerAimPosition = _gameManager.Player.View.AimPosition;
            var playerRadius = _gameManager.Player.View.Radius;

            var isCollided = VectorExtensions.IsCollided(enemyAimPosition, enemyRadius, playerAimPosition, playerRadius);
            if (!isCollided)
                return;

            var damage = _levelManager.WeaponsMap.Keys.FirstOrDefault().Model.Damage;
            var directionToPlayer = (playerAimPosition - enemyAimPosition).normalized;
            _enemy.TryToDamage(damage, directionToPlayer);

            var playerDamage = _enemy.Model.Damage;
            var directionToEnemy = (enemyAimPosition - playerAimPosition).normalized;
            _gameManager.Player.TryToDamage(playerDamage, directionToEnemy, _gameManager);
        }
    }
}
