using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public class ButtonSelectableView : UISelectableView
    {
        [SerializeField] protected Button _button;

        public Button Button => _button;

        public override void OnSelect() { }
        public override void OnDeselect() { }

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }
}

