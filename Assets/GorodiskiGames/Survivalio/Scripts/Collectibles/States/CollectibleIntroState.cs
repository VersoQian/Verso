using DG.Tweening;
using Game.Core;
using Injection;
using UnityEngine;

namespace Game.Collectible.States
{
    public sealed class CollectibleIntroState : CollectibleState
    {
        private const float _idleStateDelayMin = 0.5f;
        private const float _idleStateDelayMax = 0.8f;

        private const float _moveSpeed = 15f;

        [Inject] private Timer _timer;

        private Vector3 _endPosition;
        private float _idleStateTime;

        public CollectibleIntroState(Vector3 endPosition)
        {
            _endPosition = endPosition;
        }

        public override void Initialize()
        {
            var distance = Vector3.Distance(_collectible.View.Position, _endPosition);
            var duration = distance / _moveSpeed;

            _collectible.View.transform.DOMove(_endPosition, duration).SetEase(Ease.OutQuad).OnComplete(OnComplete).SetId(this);
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTick;
            DOTween.Kill(this);
        }

        private void OnComplete()
        {
            _idleStateTime = _timer.Time + Random.Range(_idleStateDelayMin, _idleStateDelayMax);
            _timer.TICK += OnTick;
        }

        private void OnTick()
        {
            if(_timer.Time < _idleStateTime)
                return;

            _collectible.Idle();
        }
    }
}

