using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class ShopHudView : BaseHud
    {
        [SerializeField] private ShopProductView[] _productsIAP;
        [SerializeField] private ShopProductView[] _productsExchange;

        public ShopProductView[] ProductsIAP => _productsIAP;
        public ShopProductView[] ProductsExchange => _productsExchange;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }
}

