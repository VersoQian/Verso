using Game.Unit;
using UnityEngine;

namespace Game.Bullet
{
    public sealed class StraightBulletController : BulletController
    {
        public StraightBulletController(BulletView view, BulletModel model, UnitController unit) : base(view, model, unit)
        {

        }

        protected override void Init()
        {
            SetLookRotation();
        }

        public override void Proceed()
        {
            base.Proceed();

            CalculateLifeTime();
            MoveForward();
        }

        private void MoveForward()
        {
            _view.Position += _model.MoveSpeed * Time.deltaTime * _view.transform.forward;
        }
    }
}


