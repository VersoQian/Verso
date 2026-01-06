using Injection;
using UnityEngine;
using Game.Core;
using Game.Managers;
using Utilities;

namespace Game.Collectible.States
{
    public sealed class CollectibleFlyToPlayerState : CollectibleState
    {
        private const float _moveSpeed = 15f;

        [Inject] private GameManager _gameManager;
        [Inject] private Timer _timer;
        [Inject] private LevelManager _levelManager;

        private bool _isPause;
        private float _playerRadius;
        private float _radius;

        public override void Initialize()
        {
            _playerRadius = _gameManager.Player.View.Radius;
            _radius = _collectible.View.Radius;

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

            var playerPosition = _gameManager.Player.View.Position;
            _collectible.View.Position = Vector3.MoveTowards(_collectible.View.Position, playerPosition, Time.deltaTime * _moveSpeed);

            var position = _collectible.View.Position;
            var isCollided = VectorExtensions.IsCollided(position, _radius, playerPosition, _playerRadius);
            if (!isCollided)
                return;

            _collectible.FireCollected();
        }

        private void OnPause(bool value)
        {
            _isPause = value;
        }
    }
}
