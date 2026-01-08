using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Managers;
using Game.Modules;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.Enemy.States
{
    /// <summary>
    /// 敌人改进追踪状态 - 混合射线避障 + 智能路径规划
    /// 比A*更轻量,但效果更好
    /// </summary>
    public sealed class EnemyImprovedFollowState : EnemyState
    {
        [Inject] private Timer _timer;
        [Inject] private GameManager _gameManager;
        [Inject] private LevelManager _levelManager;

        private float _walkSpeed;
        private bool _isPause;
        private TeamIDType _teamID;

        // 智能避障参数
        private const float OBSTACLE_DETECT_DISTANCE = 5f;
        private const int RAYCAST_SAMPLES = 12; // 增加采样
        private const float TURN_SMOOTHNESS = 10f; // 提高转向速度

        // 卡墙检测
        private Vector3 _lastPosition;
        private float _stuckCheckTime;
        private const float STUCK_CHECK_INTERVAL = 0.5f;
        private const float STUCK_DISTANCE_THRESHOLD = 0.1f;
        private int _stuckCounter;

        // 随机绕行
        private Vector3 _randomAvoidDirection;
        private float _randomAvoidTime;

        private LayerMask _obstacleLayer;
        private Vector3 _currentDirection;

        // 敌人间避让
        private const float SEPARATION_RADIUS = 1.5f;

        public override void Initialize()
        {
            _walkSpeed = _enemy.Model.Speed;
            _teamID = _enemy.TeamID;
            _currentDirection = _enemy.View.RotateNode.forward;
            _lastPosition = _enemy.View.Position;

            _obstacleLayer = LayerMask.GetMask("Default", "Obstacle", "Wall", "Building");

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
            if (_isPause)
                return;

            // 1. 卡墙检测
            CheckIfStuck();

            // 2. 计算朝向玩家的方向
            var toPlayer = (_gameManager.Player.View.Position - _enemy.View.Position);
            toPlayer.y = 0f;
            float distanceToPlayer = toPlayer.magnitude;
            Vector3 targetDirection = toPlayer.normalized;

            // 3. 智能避障
            Vector3 avoidDirection = CalculateSmartAvoidance(targetDirection);

            // 4. 敌人间分离
            Vector3 separationDirection = CalculateSeparation();

            // 5. 如果卡住,使用随机绕行
            if (_stuckCounter > 0)
            {
                if (Time.time - _randomAvoidTime > 2f)
                {
                    // 生成新的随机绕行方向
                    float randomAngle = Random.Range(-90f, 90f);
                    _randomAvoidDirection = Quaternion.Euler(0, randomAngle, 0) * targetDirection;
                    _randomAvoidTime = Time.time;
                    _stuckCounter = Mathf.Max(0, _stuckCounter - 1);
                }
                targetDirection = _randomAvoidDirection;
            }

            // 6. 综合方向
            Vector3 finalDirection;
            if (avoidDirection.magnitude > 0.1f)
            {
                // 有障碍物:避障70% + 目标30%
                finalDirection = (avoidDirection.normalized * 0.7f + targetDirection * 0.3f).normalized;
            }
            else
            {
                // 无障碍物:目标方向
                finalDirection = targetDirection;
            }

            // 7. 加入分离力
            finalDirection = (finalDirection * 0.8f + separationDirection * 0.2f).normalized;

            // 8. 平滑转向
            _currentDirection = Vector3.Slerp(_currentDirection, finalDirection, TURN_SMOOTHNESS * Time.deltaTime);
            _currentDirection.y = 0f;

            // 9. 移动
            if (_currentDirection.magnitude > 0.01f)
            {
                _currentDirection.Normalize();
                _enemy.View.Rotation = Quaternion.LookRotation(_currentDirection);
                _enemy.View.Position += _walkSpeed * Time.deltaTime * _currentDirection;
            }

            CheckCollisionBullets();
            CheckCollisionPlayer();
        }

        /// <summary>
        /// 智能避障 - 改进的射线检测
        /// </summary>
        private Vector3 CalculateSmartAvoidance(Vector3 targetDirection)
        {
            var position = _enemy.View.Position + Vector3.up * 0.5f;
            Vector3 bestDirection = Vector3.zero;
            float bestScore = float.MinValue;

            // 环形采样
            for (int i = 0; i < RAYCAST_SAMPLES; i++)
            {
                float angle = (360f / RAYCAST_SAMPLES) * i;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * _currentDirection;

                // 动态检测距离
                float dotToTarget = Vector3.Dot(direction, targetDirection);
                float detectDistance = Mathf.Lerp(2f, OBSTACLE_DETECT_DISTANCE, (dotToTarget + 1f) / 2f);

                RaycastHit hit;
                bool hasObstacle = Physics.Raycast(position, direction, out hit, detectDistance, _obstacleLayer);

                // 评分系统
                float score = 0f;

                if (!hasObstacle)
                {
                    // 无障碍物,评分基于朝向目标的程度
                    score = dotToTarget * 100f;
                }
                else
                {
                    // 有障碍物,距离越远分数越高
                    score = (hit.distance / detectDistance) * 50f - 50f;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = direction;
                }

                #if UNITY_EDITOR
                Debug.DrawRay(position, direction * detectDistance, hasObstacle ? Color.red : Color.green, 0f, false);
                #endif
            }

            // 如果最佳方向是障碍物,返回避障向量
            if (bestScore < 0)
            {
                return bestDirection;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 敌人间分离(避免重叠)
        /// </summary>
        private Vector3 CalculateSeparation()
        {
            Vector3 separationForce = Vector3.zero;
            int neighborCount = 0;

            var nearbyEnemies = _gameManager.Enemies
                .Where(e => e != _enemy)
                .Where(e => Vector3.Distance(e.View.Position, _enemy.View.Position) < SEPARATION_RADIUS)
                .ToList();

            foreach (var other in nearbyEnemies)
            {
                Vector3 awayFromOther = _enemy.View.Position - other.View.Position;
                float distance = awayFromOther.magnitude;

                if (distance > 0.01f && distance < SEPARATION_RADIUS)
                {
                    float strength = 1f - (distance / SEPARATION_RADIUS);
                    separationForce += awayFromOther.normalized * strength;
                    neighborCount++;
                }
            }

            if (neighborCount > 0)
            {
                separationForce /= neighborCount;
                separationForce.y = 0f;
                return separationForce.normalized;
            }

            return Vector3.zero;
        }

        /// <summary>
        /// 检测是否卡住
        /// </summary>
        private void CheckIfStuck()
        {
            if (Time.time - _stuckCheckTime < STUCK_CHECK_INTERVAL)
                return;

            _stuckCheckTime = Time.time;

            float movedDistance = Vector3.Distance(_enemy.View.Position, _lastPosition);

            if (movedDistance < STUCK_DISTANCE_THRESHOLD)
            {
                _stuckCounter++;
                if (_stuckCounter > 3)
                {
                    Debug.LogWarning($"[Enemy] 检测到卡住,启动随机绕行");
                }
            }
            else
            {
                _stuckCounter = 0;
            }

            _lastPosition = _enemy.View.Position;
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
