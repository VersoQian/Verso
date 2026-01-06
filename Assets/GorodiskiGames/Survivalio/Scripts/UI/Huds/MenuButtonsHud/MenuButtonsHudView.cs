using Game.Domain;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class MenuButtonsHudView : BaseHud
    {
        [SerializeField] private ButtonMenuView[] _buttons;
        [SerializeField] private RectTransform[] _huds;

        public ButtonMenuView[] Buttons => _buttons;
        public RectTransform[] Huds => _huds;

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }
    }
}