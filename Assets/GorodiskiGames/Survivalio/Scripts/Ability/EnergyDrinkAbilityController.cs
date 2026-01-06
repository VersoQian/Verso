using Game.Player;
using UnityEngine;

namespace Game.Ability
{
    public sealed class EnergyDrinkAbilityController : AbilityController
    {
        private const float _recoveryInterval = 5f; //in seconds

        private float _timer;

        public EnergyDrinkAbilityController(AbilityModel model) : base(model)
        {

        }

        public override void Proceed()
        {
            base.Proceed();

            _timer += Time.deltaTime;
            if(_timer < _recoveryInterval)
                return;

            RecoverHealth();
            _timer = 0f;
        }

        private void RecoverHealth()
        {
            var player = _gameManager.Player;

            var health = player.Model.GetAttribute(UnitAttributeType.Health);
            var healthNominal = player.Model.HealthNominal;

            if(health >= healthNominal)
                return;

            var healthRecovery = player.Model.GetAttribute(UnitAttributeType.HealthRecovery);
            healthRecovery /= 100f; //in percent

            var amountToRecover = healthNominal * healthRecovery;
            health = Mathf.Min(health + amountToRecover, healthNominal);

            var type = UnitAttributeType.Health;
            player.Model.SetAttribute(type, health);
            player.Model.SetChanged();
        }
    }
}

