using Game.Core;
using Game.Managers;
using Injection;
using UnityEngine;

namespace Game.Enemy.States
{
    public sealed class EnemyDieState : EnemyState
    {
        [Inject] private LevelManager _levelManager;
        [Inject] private GameManager _gameManager;
        [Inject] private Timer _timer;

        private float _endTime;

        public override void Initialize()
        {
            _enemy.View.SetCollider(false);
            _enemy.View.Die();

            var stateDuration = _enemy.View.GetCurrentStateLength;
            _endTime = Time.time + stateDuration;

            _levelManager.Model.EnemiesKilled++;
            _levelManager.Model.SetChanged();

            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTick;
        }

        public void OnTick()
        {
            if(Time.time < _endTime)
                return;

            _timer.TICK -= OnTick;

            _enemy.FireDied();
            _gameManager.FireEnemyDied(_enemy);
        }
    }
}

