using Game.Core;
using Injection;

namespace Game.Weapon.States
{
    public sealed class WeaponReadyState : WeaponState
    {
        [Inject] private Timer _timer;

        public override void Initialize()
        {
            _weapon.Model.FireRate = _weapon.Model.FireRateNominal;
            _weapon.Model.SetChanged();

            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            _weapon.FireReady();
        }
    }
}

