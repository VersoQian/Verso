using Game.UI.Hud;
using UnityEngine;

namespace Game.UI.Hud
{
    public class HudWithSelectables : BaseHud
    {
        [SerializeField] private UISelectableView[] _selectables;
        public UISelectableView[] Selectables => _selectables;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }

}
