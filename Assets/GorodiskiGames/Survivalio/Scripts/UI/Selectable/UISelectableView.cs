using UnityEngine;

namespace Game.UI.Hud
{
    public abstract class UISelectableView : BaseHud
    {
        [SerializeField] private GameObject _marker;

        public GameObject Marker => _marker;

        public abstract void OnSelect();
        public abstract void OnDeselect();
    }
}

