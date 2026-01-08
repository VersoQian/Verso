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
    /// Boss智能追踪状态 - 高级战术AI
    /// 特性:
    /// - 保持战斗距离(近战/远程)
    /// - 包抄绕后策略
    /// - 撤退/拉扯战术
    /// - 位置评估系统
    /// </summary>
    public sealed class BossSmartFollowState : EnemyState
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
        private const float PATH_REPLAN_INTERVAL = 0.8f; // Boss每0.8秒重新规划
        private const float WAYPOINT_REACHED_DISTANCE = 0.5f;
        private float _lastReplanTime;

        // Boss战术参数
        private const float PREFERRED_DISTANCE_MIN = 5f; // 偏好距离(远程Boss保持距离)
        private const float PREFERRED_DISTANCE_MAX = 8f;
        private const float RETREAT_DISTANCE = 3f; // 撤退距离(太近时撤退)
        private const float FLANK_CHANCE = 0.3f; // 包抄概率
        private BossTacticMode _currentTactic = BossTacticMode.Approach;
        private float _tacticDecisionCooldown;
        private const float TACTIC_DECISION_INTERVAL = 2f; // 每2秒重新决策战术

        // 平滑转向
        private const float TURN_SMOOTHNESS = 6f;
        private Vector3 _currentDirection;

        private enum BossTacticMode
        {
            Approach,   // 接近模式
            Maintain,   // 保持距离
            Retreat,    // 撤退模式
            Flank       // 包抄模式
        }

        public override void Initialize()
        {
            _walkSpeed = _enemy.Model.Speed;
            _teamID = _enemy.TeamID;
            _currentDirection = _enemy.View.RotateNode.forward;

            // 自动获取或创建寻路网格
            _pathfindingGrid = PathfindingManager.Instance.Grid;

            if (_pathfindingGrid == null || !_pathfindingGrid.IsInitialized)
            {
                Debug.LogError("[BossSmartFollowState] PathfindingGrid初始化失败! 请检查场景配置");
                // 降级到简单移动模式
                return;
            }

            _pathfinder = new AStarPathfinder(_pathfindingGrid);

            // 立即规划第一次路径
            DecideTacticAndPlan();

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

            // 定期重新决策战术并规划路径
            if (_timer.Time - _tacticDecisionCooldown > TACTIC_DECISION_INTERVAL)
            {
                DecideTacticAndPlan();
            }
            else if (_timer.Time - _lastReplanTime > PATH_REPLAN_INTERVAL)
            {
                // 在当前战术下重新规划路径
                ReplanPath(_currentTactic);
            }

            // 沿路径移动
            if (_currentPath != null && _currentPath.Count > 0)
            {
                FollowPath();
            }
            else
            {
                // 没有路径,使用简单移动
                MoveWithCurrentTactic();
            }

            CheckCollisionBullets();
            CheckCollisionPlayer();
        }

        /// <summary>
        /// 决策战术模式
        /// </summary>
        private void DecideTacticAndPlan()
        {
            _tacticDecisionCooldown = _timer.Time;

            float distanceToPlayer = Vector3.Distance(_enemy.View.Position, _gameManager.Player.View.Position);

            // 战术决策逻辑
            if (distanceToPlayer < RETREAT_DISTANCE)
            {
                // 太近了,撤退!
                _currentTactic = BossTacticMode.Retreat;
            }
            else if (distanceToPlayer < PREFERRED_DISTANCE_MIN)
            {
                // 距离合适,保持距离
                _currentTactic = BossTacticMode.Maintain;
            }
            else if (distanceToPlayer > PREFERRED_DISTANCE_MAX)
            {
                // 太远了,接近
                _currentTactic = BossTacticMode.Approach;
            }
            else
            {
                // 在理想距离范围内,随机选择包抄或保持
                if (Random.value < FLANK_CHANCE)
                {
                    _currentTactic = BossTacticMode.Flank;
                }
                else
                {
                    _currentTactic = BossTacticMode.Maintain;
                }
            }

            Debug.Log($"[Boss] 战术决策: {_currentTactic}, 距离玩家: {distanceToPlayer:F1}m");

            ReplanPath(_currentTactic);
        }

        /// <summary>
        /// 根据战术模式规划路径
        /// </summary>
        private void ReplanPath(BossTacticMode tactic)
        {
            _lastReplanTime = _timer.Time;

            Vector3 startPos = _enemy.View.Position;
            Vector3 playerPos = _gameManager.Player.View.Position;
            Vector3 targetPos = playerPos;

            switch (tactic)
            {
                case BossTacticMode.Approach:
                    // 直接接近玩家
                    targetPos = playerPos;
                    break;

                case BossTacticMode.Maintain:
                    // 保持当前距离,环绕移动
                    targetPos = CalculateCircleAroundPlayer(playerPos, PREFERRED_DISTANCE_MIN + 1f);
                    break;

                case BossTacticMode.Retreat:
                    // 撤退:朝玩家相反方向
                    Vector3 retreatDirection = (startPos - playerPos).normalized;
                    targetPos = playerPos + retreatDirection * (PREFERRED_DISTANCE_MAX + 2f);
                    break;

                case BossTacticMode.Flank:
                    // 包抄:从侧面接近玩家
                    targetPos = CalculateFlankPosition(playerPos);
                    break;
            }

            // 使用A*寻找路径
            _currentPath = _pathfinder.FindPath(startPos, targetPos, smoothPath: true);

            if (_currentPath != null && _currentPath.Count > 0)
            {
                _currentWaypointIndex = 0;

                #if UNITY_EDITOR
                // 调试可视化路径
                Color pathColor = tactic == BossTacticMode.Retreat ? Color.yellow :
                                 tactic == BossTacticMode.Flank ? Color.magenta : Color.cyan;
                for (int i = 0; i < _currentPath.Count - 1; i++)
                {
                    Debug.DrawLine(_currentPath[i] + Vector3.up, _currentPath[i + 1] + Vector3.up, pathColor, PATH_REPLAN_INTERVAL);
                }
                #endif
            }
        }

        /// <summary>
        /// 计算环绕玩家的位置
        /// </summary>
        private Vector3 CalculateCircleAroundPlayer(Vector3 playerPos, float radius)
        {
            Vector3 currentOffset = _enemy.View.Position - playerPos;
            currentOffset.y = 0f;

            if (currentOffset.magnitude < 0.1f)
            {
                currentOffset = Random.insideUnitSphere;
                currentOffset.y = 0f;
            }

            // 顺时针或逆时针旋转45度
            float angle = Random.value > 0.5f ? 45f : -45f;
            Vector3 newDirection = Quaternion.Euler(0, angle, 0) * currentOffset.normalized;

            return playerPos + newDirection * radius;
        }

        /// <summary>
        /// 计算包抄位置(侧面)
        /// </summary>
        private Vector3 CalculateFlankPosition(Vector3 playerPos)
        {
            Vector3 toPlayer = playerPos - _enemy.View.Position;
            toPlayer.y = 0f;

            // 向玩家的左侧或右侧偏移90度
            float angle = Random.value > 0.5f ? 90f : -90f;
            Vector3 flankDirection = Quaternion.Euler(0, angle, 0) * toPlayer.normalized;

            return playerPos + flankDirection * PREFERRED_DISTANCE_MIN;
        }

        /// <summary>
        /// 沿路径移动
        /// </summary>
        private void FollowPath()
        {
            if (_currentWaypointIndex >= _currentPath.Count)
            {
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

            // 移动
            Vector3 moveDirection = toWaypoint.normalized;
            _currentDirection = Vector3.Slerp(_currentDirection, moveDirection, TURN_SMOOTHNESS * Time.deltaTime);
            _currentDirection.y = 0f;
            _currentDirection.Normalize();

            if (_currentDirection != Vector3.zero)
            {
                _enemy.View.Rotation = Quaternion.LookRotation(_currentDirection);

                // Boss在撤退时移动速度提升20%
                float speedMultiplier = _currentTactic == BossTacticMode.Retreat ? 1.2f : 1f;
                _enemy.View.Position += _walkSpeed * speedMultiplier * Time.deltaTime * _currentDirection;
            }
        }

        /// <summary>
        /// 简单移动(无路径时的备用方案)
        /// </summary>
        private void MoveWithCurrentTactic()
        {
            Vector3 toPlayer = (_gameManager.Player.View.Position - _enemy.View.Position);
            toPlayer.y = 0f;

            Vector3 moveDirection = Vector3.zero;

            switch (_currentTactic)
            {
                case BossTacticMode.Approach:
                    moveDirection = toPlayer.normalized;
                    break;

                case BossTacticMode.Retreat:
                    moveDirection = -toPlayer.normalized;
                    break;

                case BossTacticMode.Maintain:
                case BossTacticMode.Flank:
                    // 保持距离或包抄时,如果没有路径就不移动
                    return;
            }

            _currentDirection = Vector3.Slerp(_currentDirection, moveDirection, TURN_SMOOTHNESS * Time.deltaTime);
            _currentDirection.y = 0f;
            _currentDirection.Normalize();

            if (_currentDirection != Vector3.zero)
            {
                _enemy.View.Rotation = Quaternion.LookRotation(_currentDirection);
                _enemy.View.Position += _walkSpeed * Time.deltaTime * _currentDirection;
            }
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
