using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class PurchaseEnergyHudView : BaseHud
    {
        [SerializeField] private Button _closeButton;
        [SerializeField] private ShopProductView _productExchange;

        public Button CloseButon => _closeButton;
        public ShopProductView ProductExchange => _productExchange;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }
}
