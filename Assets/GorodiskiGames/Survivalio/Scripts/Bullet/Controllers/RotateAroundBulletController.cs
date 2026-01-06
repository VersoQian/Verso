using Game.Unit;
using UnityEngine;

namespace Game.Bullet
{
    public sealed class RotateAroundBulletController : BulletController
    {
        private const float _orbitSpeed = -180f;
        private const float _spinSpeed = 2.5f * -360f;

        private float _baseAngle;
        private float _orbitRadius;

        public RotateAroundBulletController(BulletView view, BulletModel model, UnitController unit) : base(view, model, unit)
        {

        }

        protected override void Init()
        {
            float angleStep = 360f / _model.Numbers;
            _baseAngle = _model.Number * angleStep;
            _orbitRadius = _model.OrbitRadius;

            RotateAround();
        }

        public override void OnUnitDied()
        {
            base.OnUnitDied();
            FireLifetimeEnd();
        }

        public override void FireCollideEnemy()
        {

        }

        public override void Proceed()
        {
            base.Proceed();

            CalculateLifeTime();
            RotateAround();
            Spin();
        }

        private void Spin()
        {
            var rotationAmount = _spinSpeed * Time.deltaTime;
            _view.transform.Rotate(0, rotationAmount, 0, Space.Self);
        }

        private void RotateAround()
        {
            float timeAngle = _orbitSpeed * Time.time;
            float angle = _baseAngle + timeAngle;
            float rad = angle * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(Mathf.Cos(rad), 0f, Mathf.Sin(rad)) * _orbitRadius;
            _view.Position = _unit.View.AimPosition + offset;
        }
    }
}

