using Game.Enemy;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class BossIncomingHudView : BaseHudWithModel<EnemyModel>
    {
        [SerializeField] private Image _healthImage;
        [SerializeField] private GameObject _infoHolder;

        public GameObject InfoHolder => _infoHolder;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(EnemyModel model)
        {
            _healthImage.fillAmount = (float)model.Health / model.HealthNominal;
        }
    }
}

