using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class DamageBorderHudView : BaseHud
    {
        [SerializeField] private Image _image;

        protected override void OnEnable()
        {
        }

        protected override void OnDisable()
        {
        }

        public void SetColor(Color color)
        {
            _image.color = color;
        }
    }
}