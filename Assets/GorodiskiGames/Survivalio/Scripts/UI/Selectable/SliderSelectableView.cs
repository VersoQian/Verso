using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class SliderSelectableView : UISelectableView
    {
        private const float _step = 0.05f;

        [SerializeField] private Slider _slider;

        public Slider Slider => _slider;

        public override void OnSelect() { }
        public override void OnDeselect() { }

        public void Increase()
        {
            _slider.value = Mathf.Clamp01(_slider.value + _step);
        }

        public void Decrease()
        {
            _slider.value = Mathf.Clamp01(_slider.value - _step);
        }

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }
}
