using Game.Core;
using Game.Managers;
using Game.Player;
using Injection;
using UnityEngine;

namespace Game.Collectible.States
{
    public class CollectibleIdleState : CollectibleState
    {
        [Inject] private Timer _timer;
        [Inject] private GameManager _gameManager;
        [Inject] private LevelManager _levelManager;

        private UnitAttributeType _collectDistanceAttribute;
        private float _collectDistance;
        private bool _isPause;


        public override void Initialize()
        {
            _collectDistanceAttribute = UnitAttributeType.CollectDistance;
            OnAttributeUpdated(_collectDistanceAttribute);

            _levelManager.ON_ATTRIBUTE_UPDATED += OnAttributeUpdated;
            _levelManager.ON_PAUSE += OnPause;

            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _levelManager.ON_ATTRIBUTE_UPDATED -= OnAttributeUpdated;
            _levelManager.ON_PAUSE -= OnPause;

            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            if (_isPause)
                return;

            var distanceToPlayer = Vector3.Distance(_gameManager.Player.View.Position, _collectible.View.Position);
            if (distanceToPlayer > _collectDistance)
                return;

            _collectible.FlyToPlayer();
        }

        private void OnAttributeUpdated(UnitAttributeType attributeType)
        {
            if(attributeType != _collectDistanceAttribute)
                return;

            _collectDistance = _gameManager.Player.Model.GetAttribute(_collectDistanceAttribute);
        }

        private void OnPause(bool value)
        {
            _isPause = value;
        }
    }
}

