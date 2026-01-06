using UnityEngine;
using Game.Unit;
using Utilities;
using Injection;
using Game.Managers;
using Game.Effect;

namespace Game.Bullet
{
    public sealed class ParabolaBulletController : BulletController
    {
        private const float _parabolaHeightMultiplier = 0.5f;
        private const float _rotateSpeed = 2f * 360f;

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
            _endPosition = AimPosition;
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
    }
}

