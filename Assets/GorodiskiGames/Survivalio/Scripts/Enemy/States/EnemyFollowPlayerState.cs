using System.Linq;
using Game.Core;
using Game.Managers;
using Game.Modules;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.Enemy.States
{
    public sealed class EnemyFollowPlayerState : EnemyState
    {
        [Inject] private Timer _timer;
        [Inject] private GameManager _gameManager;
        [Inject] private LevelManager _levelManager;

        private float _walkSpeed;
        private bool _isPause;
        private TeamIDType _teamID;

        // 高级避障参数
        private const float OBSTACLE_DETECT_DISTANCE = 3f; // 前方检测距离
        private const float SIDE_DETECT_DISTANCE = 2f; // 侧面检测距离
        private const int RAYCAST_SAMPLES = 8; // 环形射线采样数量
        private const float TURN_SMOOTHNESS = 5f; // 转向平滑度
        private const float AVOIDANCE_WEIGHT = 0.8f; // 避障权重
        private const float TARGET_WEIGHT = 0.2f; // 目标追踪权重（避障时）
        private const float OBSTACLE_REPULSION_STRENGTH = 2f; // 障碍物排斥力强度

        private LayerMask _obstacleLayer;
        private Vector3 _currentDirection; // 当前移动方向（平滑）
        private Vector3 _desiredDirection; // 期望移动方向

        public override void Initialize()
        {
            _walkSpeed = _enemy.Model.Speed;
            _teamID = _enemy.TeamID;
            _currentDirection = _enemy.View.RotateNode.forward;
            _desiredDirection = _currentDirection;

            // 设置障碍物图层
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
            if(_isPause)
                return;

            // 1. 计算朝向玩家的理想方向
            var toPlayer = (_gameManager.Player.View.Position - _enemy.View.Position).normalized;
            toPlayer.y = 0f;

            // 2. 使用环形射线检测获取避障向量场
            Vector3 avoidanceVector = CalculateAvoidanceVectorField();

            // 3. 混合目标方向和避障方向
            if (avoidanceVector != Vector3.zero)
            {
                // 有障碍物，混合避障和目标追踪
                _desiredDirection = (avoidanceVector * AVOIDANCE_WEIGHT + toPlayer * TARGET_WEIGHT).normalized;
            }
            else
            {
                // 无障碍物，直接朝向玩家
                _desiredDirection = toPlayer;
            }

            // 4. 平滑转向（避免突然转向）
            _currentDirection = Vector3.Slerp(_currentDirection, _desiredDirection, TURN_SMOOTHNESS * Time.deltaTime);
            _currentDirection.y = 0f;
            _currentDirection.Normalize();

            // 5. 应用移动和旋转
            if (_currentDirection != Vector3.zero)
            {
                _enemy.View.Rotation = Quaternion.LookRotation(_currentDirection);
                _enemy.View.Position += _walkSpeed * Time.deltaTime * _currentDirection;
            }

            CheckCollisionBullets();
            CheckCollisionPlayer();
        }

        /// <summary>
        /// 使用环形多射线采样计算避障向量场
        /// 基于Velocity Obstacle算法，计算综合避障方向
        /// </summary>
        private Vector3 CalculateAvoidanceVectorField()
        {
            var position = _enemy.View.Position + Vector3.up * 0.5f; // 提高检测点，避免地面干扰
            Vector3 repulsionForce = Vector3.zero;
            int hitCount = 0;

            // 环形采样：360度均匀分布射线
            for (int i = 0; i < RAYCAST_SAMPLES; i++)
            {
                float angle = (360f / RAYCAST_SAMPLES) * i;
                Vector3 direction = Quaternion.Euler(0, angle, 0) * _currentDirection;

                // 计算检测距离（前方远，后方近）
                float dotProduct = Vector3.Dot(direction, _currentDirection);
                float detectDistance = Mathf.Lerp(SIDE_DETECT_DISTANCE, OBSTACLE_DETECT_DISTANCE, (dotProduct + 1f) / 2f);

                RaycastHit hit;
                if (Physics.Raycast(position, direction, out hit, detectDistance, _obstacleLayer))
                {
                    hitCount++;

                    // 计算排斥力：距离越近，排斥力越强
                    float distanceRatio = 1f - (hit.distance / detectDistance);
                    float repulsionStrength = distanceRatio * distanceRatio * OBSTACLE_REPULSION_STRENGTH;

                    // 排斥方向：从障碍物指向敌人
                    Vector3 awayFromObstacle = (position - hit.point).normalized;
                    awayFromObstacle.y = 0f;

                    // 累加排斥力
                    repulsionForce += awayFromObstacle * repulsionStrength;

                    #if UNITY_EDITOR
                    Debug.DrawLine(position, hit.point, Color.red);
                    #endif
                }
                else
                {
                    #if UNITY_EDITOR
                    Debug.DrawRay(position, direction * detectDistance, Color.green, 0f, false);
                    #endif
                }
            }

            // 没有检测到障碍物
            if (hitCount == 0)
                return Vector3.zero;

            // 归一化排斥力向量
            repulsionForce.y = 0f;
            if (repulsionForce.magnitude > 0.01f)
            {
                repulsionForce = repulsionForce.normalized;

                #if UNITY_EDITOR
                Debug.DrawRay(position, repulsionForce * 2f, Color.yellow);
                #endif
            }

            return repulsionForce;
        }

        /// <summary>
        /// 计算从当前方向到目标方向的最短旋转角度（带符号）
        /// </summary>
        private float GetSignedAngle(Vector3 from, Vector3 to)
        {
            float angle = Vector3.Angle(from, to);
            Vector3 cross = Vector3.Cross(from, to);
            if (cross.y < 0) angle = -angle;
            return angle;
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
                if(type == Config.BulletControllerType.Parabola)
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

                //enemy damage
                var damage = bullet.Model.Damage;
                var direction = (bulletPosition - aimPosition).normalized;
                _enemy.TryToDamage(damage, direction);
                //end enemy damage
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

            //enemy damage
            var damage = _levelManager.WeaponsMap.Keys.FirstOrDefault().Model.Damage;
            var directionToPlayer = (playerAimPosition - enemyAimPosition).normalized;
            _enemy.TryToDamage(damage, directionToPlayer);
            //end enemy damage

            //player damage
            var playerDamage = _enemy.Model.Damage;
            var directionToEnemy = (enemyAimPosition - playerAimPosition).normalized;
            _gameManager.Player.TryToDamage(playerDamage, directionToEnemy, _gameManager);
            //end player damage
        }
    }
}

