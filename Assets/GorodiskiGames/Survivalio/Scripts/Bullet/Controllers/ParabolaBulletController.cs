using UnityEngine;
using Game.Unit;
using Utilities;
using Injection;
using Game.Managers;
using Game.Effect;
using System.Collections.Generic;
using Game.Enemy;

namespace Game.Bullet
{
    public sealed class ParabolaBulletController : BulletController
    {
        private const float _parabolaHeightMultiplier = 0.5f;
        private const float _rotateSpeed = 2f * 360f;

        // 调试开关:在编辑器中可视化手榴弹瞄准逻辑
        private const bool _debugVisualization = true;

        [Inject] private GameManager _gameManager;

        private Vector3 _startPosition;
        private Vector3 _endPosition;
        private float _elapsed;
        private float _height;
        private float _duration;
        private int _rotateSign;

        public ParabolaBulletController(BulletView view, BulletModel model, UnitController unit) : base(view, model, unit)
        {

        }

        protected override void Init()
        {
            _rotateSign = MathUtil.RandomSign;
            _startPosition = InitPosition;
            _endPosition = GetSmartLandingPoint(AimPosition);
            var distance = Vector3.Distance(_startPosition, _endPosition);
            _height = distance * _parabolaHeightMultiplier;

            var parabolaLength = MathUtil.GetParabolaLength(_startPosition, _endPosition, _height);
            _duration = parabolaLength / _model.MoveSpeed;
        }

        public override void FireCollideEnemy()
        {

        }

        public override void Proceed()
        {
            base.Proceed();
            MoveParabola();
            Rotate();
        }

        private void MoveParabola()
        {
            _elapsed += Time.deltaTime;

            var t = Mathf.Clamp01(_elapsed / _duration);

            var parabolaPosition = MathUtil.GetParabolaPoint(_startPosition, _endPosition, _height, t);
            _view.Position = parabolaPosition;

            if (_elapsed < _duration)
                return;

            _view.Position = _endPosition;

            FireLifetimeEnd();
        }

        private void Rotate()
        {
            var rotationAmount = _rotateSpeed * Time.deltaTime * Vector3.one * _rotateSign;
            _view.LocalTransform.Rotate(rotationAmount, Space.Self);
        }

        public override void FireLifetimeEnd()
        {
            base.FireLifetimeEnd();

            var position = _view.Position;
            _gameManager.FireSpawnExplosion(position, _model.Damage);
        }

        /// <summary>
        /// 智能预判落点：优先追踪Boss,其次选取敌人最密集的位置（带速度预判）
        /// </summary>
        private Vector3 GetSmartLandingPoint(Vector3 fallback)
        {
            if (_gameManager == null || _gameManager.Enemies == null || _gameManager.Enemies.Count == 0)
                return fallback;

            // 爆炸半径来自 ExplosionView 默认配置，若后续修改可同步调整
            const float explosionRadius = 5f;
            const float distancePenaltyFactor = 0.03f;  // 降低距离惩罚,让远处密集区域也能被选中
            const float healthWeightFactor = 0.1f;      // 血量权重:优先打低血量敌人
            const float bossScoreMultiplier = 10f;      // Boss得分倍数:非常优先Boss
            const int maxEnemiesCheck = 50;             // 限制检查敌人数量,避免性能问题

            var bestScore = float.MinValue;
            var bestPosition = fallback;

            // 🎯 第一优先级: 检查是否有Boss存在
            EnemyController targetBoss = null;
            for (int i = 0; i < _gameManager.Enemies.Count; i++)
            {
                var enemy = _gameManager.Enemies[i];
                if (enemy != null && enemy.IsBoss)
                {
                    targetBoss = enemy;
                    break; // 找到Boss立即锁定
                }
            }

            // 如果有Boss,直接瞄准Boss的预测位置
            if (targetBoss != null && targetBoss.View != null)
            {
                var bossPos = targetBoss.View.Position;
                var flightTime = EstimateFlightTime(bossPos);

                // Boss移动预判
                var velocity = Vector3.zero;
                if (targetBoss.Model.Speed > 0.1f)
                {
                    var forward = targetBoss.View.Rotation * Vector3.forward;
                    velocity = forward.normalized * targetBoss.Model.Speed * flightTime;
                }

                var predictedBossPos = bossPos + velocity;
                predictedBossPos.y = _startPosition.y;

                // 检查射程
                var distance = Vector3.Distance(_startPosition, predictedBossPos);
                if (_model.Range <= 0f || distance <= _model.Range * 1.5f)
                {
#if UNITY_EDITOR
                    // Boss追踪可视化
                    if (_debugVisualization)
                    {
                        Debug.DrawLine(_startPosition, predictedBossPos, Color.magenta, 2f);

                        // 绘制Boss爆炸范围(用紫色)
                        const int segments = 24;
                        for (int i = 0; i < segments; i++)
                        {
                            float angle1 = i * 360f / segments * Mathf.Deg2Rad;
                            float angle2 = (i + 1) * 360f / segments * Mathf.Deg2Rad;

                            var p1 = predictedBossPos + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * explosionRadius;
                            var p2 = predictedBossPos + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * explosionRadius;

                            Debug.DrawLine(p1, p2, Color.magenta, 2f);
                        }
                    }
#endif
                    return predictedBossPos; // 直接返回Boss位置,忽略其他目标
                }
            }

            // 🎯 第二优先级: 没有Boss或Boss超出射程,寻找最佳群攻位置
            var enemyCount = Mathf.Min(_gameManager.Enemies.Count, maxEnemiesCheck);
            var predictedPositions = new Vector3[enemyCount];
            var validEnemies = new List<EnemyController>(enemyCount);

            // 第一遍:预计算所有敌人的未来位置
            int validIndex = 0;
            for (int i = 0; i < _gameManager.Enemies.Count && validIndex < maxEnemiesCheck; i++)
            {
                var enemy = _gameManager.Enemies[i];
                if (enemy == null || enemy.View == null)
                    continue;

                var basePos = enemy.View.Position;
                var flightTime = EstimateFlightTime(basePos);

                // 速度预判:考虑敌人移动方向
                var velocity = Vector3.zero;
                if (enemy.Model.Speed > 0.1f)
                {
                    var forward = enemy.View.Rotation * Vector3.forward;
                    velocity = forward.normalized * enemy.Model.Speed * flightTime;
                }

                var predicted = basePos + velocity;
                predicted.y = _startPosition.y;

                var distance = Vector3.Distance(_startPosition, predicted);
                if (_model.Range > 0f && distance > _model.Range * 1.5f) // 稍微放宽射程限制
                    continue;

                predictedPositions[validIndex] = predicted;
                validEnemies.Add(enemy);
                validIndex++;
            }

            // 第二遍:评估每个候选落点的价值
            for (int i = 0; i < validIndex; i++)
            {
                var candidatePos = predictedPositions[i];
                var targetEnemy = validEnemies[i];

                // 计算该落点能覆盖的敌人数量和总价值
                float totalValue = 0f;
                int clusteredCount = 0;
                bool hasNearbyBoss = false; // 检查范围内是否有Boss

                for (int j = 0; j < validIndex; j++)
                {
                    var otherPos = predictedPositions[j];
                    var otherEnemy = validEnemies[j];

                    var distToCandidate = Vector3.Distance(otherPos, candidatePos);

                    if (distToCandidate <= explosionRadius)
                    {
                        clusteredCount++;

                        // 如果这个位置能炸到Boss,极大提升得分
                        if (otherEnemy.IsBoss)
                        {
                            hasNearbyBoss = true;
                            totalValue += bossScoreMultiplier; // Boss额外加分
                        }

                        // 价值计算:基础价值1 + 血量权重(优先低血量)
                        var healthRatio = (float)otherEnemy.Model.Health / Mathf.Max(1f, otherEnemy.Model.HealthNominal);
                        var enemyValue = 1f + (1f - healthRatio) * healthWeightFactor;

                        // 距离爆炸中心越近价值越高
                        var centerDistanceFactor = 1f - (distToCandidate / explosionRadius * 0.3f);
                        totalValue += enemyValue * centerDistanceFactor;
                    }
                }

                // 综合得分 = 总价值 - 距离惩罚
                var distanceFromStart = Vector3.Distance(_startPosition, candidatePos);
                var distancePenalty = distanceFromStart * distancePenaltyFactor;

                // 聚集度加成:3个以上敌人时额外加分
                var clusterBonus = clusteredCount >= 3 ? (clusteredCount - 2) * 0.5f : 0f;

                var finalScore = totalValue + clusterBonus - distancePenalty;

                if (finalScore > bestScore)
                {
                    bestScore = finalScore;
                    bestPosition = candidatePos;
                }
            }

            // 如果没找到好目标(得分太低),使用fallback
            if (bestScore < 0.5f)
                return fallback;

#if UNITY_EDITOR
            // 可视化调试:在Scene视图中绘制爆炸范围和命中的敌人
            if (_debugVisualization)
            {
                Debug.DrawLine(_startPosition, bestPosition, Color.yellow, 2f);

                // 绘制爆炸范围圆圈(用线段近似)
                const int segments = 24;
                for (int i = 0; i < segments; i++)
                {
                    float angle1 = i * 360f / segments * Mathf.Deg2Rad;
                    float angle2 = (i + 1) * 360f / segments * Mathf.Deg2Rad;

                    var p1 = bestPosition + new Vector3(Mathf.Cos(angle1), 0, Mathf.Sin(angle1)) * explosionRadius;
                    var p2 = bestPosition + new Vector3(Mathf.Cos(angle2), 0, Mathf.Sin(angle2)) * explosionRadius;

                    Debug.DrawLine(p1, p2, Color.red, 2f);
                }

                // 绘制命中的敌人连线
                for (int i = 0; i < validIndex; i++)
                {
                    if (Vector3.Distance(predictedPositions[i], bestPosition) <= explosionRadius)
                    {
                        Debug.DrawLine(bestPosition, predictedPositions[i], Color.green, 2f);
                    }
                }
            }
#endif

            return bestPosition;
        }

        private float EstimateFlightTime(Vector3 targetPosition)
        {
            var distance = Vector3.Distance(_startPosition, targetPosition);
            var height = distance * _parabolaHeightMultiplier;
            var arcLength = MathUtil.GetParabolaLength(_startPosition, targetPosition, height);
            return arcLength / Mathf.Max(0.01f, _model.MoveSpeed);
        }
    }
}
