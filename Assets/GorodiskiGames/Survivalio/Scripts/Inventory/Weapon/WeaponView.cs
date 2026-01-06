using Core;
using UnityEngine;
using UnityEngine.UI;

namespace Game.Weapon
{
    public sealed class WeaponView : BehaviourWithModel<WeaponModel>
    {
        [SerializeField] private Image _fireRate;

        protected override void OnModelChanged(WeaponModel model)
        {
            _fireRate.fillAmount = model.FireRate / model.FireRateNominal;
        }
    }
}

