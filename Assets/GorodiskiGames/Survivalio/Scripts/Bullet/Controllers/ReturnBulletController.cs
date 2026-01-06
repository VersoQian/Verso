using System.Collections;
using Game.Unit;
using UnityEngine;

namespace Game.Bullet
{
    public sealed class ReturnBulletController : BulletController
    {
        private const float _spinSpeed = 3f * 360f;

        public ReturnBulletController(BulletView view, BulletModel model, UnitController unit) : base(view, model, unit)
        {

        }

        protected override void Init()
        {
            _view.StartCoroutine(MoveForward());
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
            Spin();
        }

        private void Spin()
        {
            var rotationAmount = _spinSpeed * Time.deltaTime;
            _view.LocalTransform.Rotate(0, rotationAmount, 0, Space.Self);
        }

        private IEnumerator MoveForward()
        {
            Vector3 startPos = _view.Position;
            Vector3 targetPos = startPos + _view.transform.forward * _model.Range;
            float distance = Vector3.Distance(startPos, targetPos);
            float duration = distance / _model.MoveSpeed;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                while (_isPause)
                    yield return null;

                _view.Position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _view.Position = targetPos;
            _view.StartCoroutine(MoveBackward());
        }

        private IEnumerator MoveBackward()
        {
            Vector3 startPos = _view.Position;
            Vector3 direction = (_unit.View.AimPosition - startPos).normalized;
            float delta = _model.Range * 2f;
            Vector3 targetPos = _view.Position + direction * delta;
            var distance = Vector3.Distance(startPos, targetPos);
            var duration = distance / _model.MoveSpeed;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                while (_isPause)
                    yield return null;

                _view.Position = Vector3.Lerp(startPos, targetPos, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }
            _view.Position = targetPos;

            _view.StartCoroutine(MoveBackToPlayer());
        }

        private IEnumerator MoveBackToPlayer()
        {
            Vector3 startPos = _view.Position;
            var distance = Vector3.Distance(startPos, _unit.View.AimPosition);
            var duration = distance / _model.MoveSpeed;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                while (_isPause)
                    yield return null;

                _view.Position = Vector3.Lerp(startPos, _unit.View.AimPosition, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _view.Position = _unit.View.AimPosition;

            FireLifetimeEnd();
        }
    }
}

