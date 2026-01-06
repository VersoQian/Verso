using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class PurchaseHudView : BaseHud
    {
        [SerializeField] private GameObject _backgroundImage;
        [SerializeField] private TMP_Text _infoText;

        public GameObject BackgroundImage => _backgroundImage;
        public TMP_Text InfoText => _infoText;

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}

