using System.Collections.Generic;
using Game.Config;
using Game.Core.UI;
using Game.Managers;
using Injection;

namespace Game.UI.Hud
{
    public sealed class PurchaseEnergyHudMediator : Mediator<PurchaseEnergyHudView>
    {
        private const string _priceFormat = "{0} {1}";

        [Inject] private HudManager _hudManager;
        [Inject] private MenuManager _menuManager;

        private Dictionary<string, ShopProductView> _productMap;

        public PurchaseEnergyHudMediator()
        {
            _productMap = new Dictionary<string, ShopProductView>();
        }

        protected override void Show()
        {
            SetProductExchange();

            _view.CloseButon.onClick.AddListener(OnCloseButtonClick);
        }

        protected override void Hide()
        {
            _view.CloseButon.onClick.RemoveListener(OnCloseButtonClick);

            foreach (var product in _productMap.Values)
            {
                product.ON_CLICK -= OnProductClick;
            }
            _productMap.Clear();
        }

        private void OnCloseButtonClick()
        {
            _hudManager.HideSingle();
        }

        private void SetProductExchange()
        {
            var product = _view.ProductExchange;
            var config = product.Config as ShopProductExchangeConfig;
            var productID = config.ID;

            var priceAmount = config.Price.Amount;

            var priceIcon = GameConstants.PassCardIcon;
            var resourceType = config.Price.ResourceType;
            if (resourceType == ResourceItemType.Credit)
                priceIcon = GameConstants.CreditIcon;

            var priceResult = string.Format(_priceFormat, priceIcon, priceAmount);
            var amountResult = config.Reward.Amount.ToString();

            _productMap.Add(productID, product);

            product.Initialize(priceResult, amountResult);
            product.ON_CLICK += OnProductClick;
        }

        private void OnProductClick(string productID)
        {
            var product = _productMap[productID];
            var type = product.Config.ProductType;

            if (type == ShopProductType.ResourcesExchange)
                TryExchange(product);
        }

        private void TryExchange(ShopProductView product)
        {
            var config = product.Config as ShopProductExchangeConfig;

            var priceType = config.Price.ResourceType;
            var priceAmount = config.Price.Amount;

            var rewardType = config.Reward.ResourceType;
            var rewardAmount = config.Reward.Amount;

            if (priceType == ResourceItemType.PassCard && _menuManager.Model.PassCard >= priceAmount)
            {
                _menuManager.Model.PassCard -= priceAmount;
                _menuManager.Model.SaveResource(rewardType, rewardAmount);
            }
        }
    }
}
