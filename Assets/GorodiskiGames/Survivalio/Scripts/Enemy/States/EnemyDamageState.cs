using Game.Config;
using Game.Core;
using Injection;
using UnityEngine;

namespace Game.Enemy.States
{
    public class EnemyDamageState : EnemyState
    {
        private const float _distance = 1.5f;
        private const float _speed = 15f;

        [Inject] protected Timer _timer;
        [Inject] protected GameConfig _config;

        private Vector3 _targetPosition;

        public override void Initialize()
        {
            var direction = -_enemy.View.RotateNode.forward;
            _targetPosition = _enemy.View.Position + direction.normalized * _distance;

            var blinkDuration = _distance / _speed * 0.5f;
            _enemy.View.Damage(blinkDuration);

            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            _enemy.View.Position = Vector3.MoveTowards(_enemy.View.Position, _targetPosition, _speed * Time.deltaTime);

            if (Vector3.Distance(_enemy.View.Position, _targetPosition) > 0.05f)
                return;

            OnEnd();
        }

        protected virtual void OnEnd()
        {
            _enemy.FollowPlayer();
        }
    }
}

