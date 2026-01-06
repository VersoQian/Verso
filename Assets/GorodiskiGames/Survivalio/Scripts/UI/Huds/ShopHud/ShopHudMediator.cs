using System.Collections.Generic;
using Game.Config;
using Game.Core.UI;
using Game.Managers;
using Injection;

namespace Game.UI.Hud
{
    public sealed class ShopHudMediator : Mediator<ShopHudView>
    {
        private const string _priceFormat = "{0} {1}";

        [Inject] private MenuManager _menuManager;
        [Inject] private IAPManager _IAPManager;
        [Inject] private GameConfig _config;

        private Dictionary<string, ShopProductView> _productMap;

        public ShopHudMediator()
        {
            _productMap = new Dictionary<string, ShopProductView>();
        }

        protected override void Show()
        {
            SetProductsIAP();
            SetProductsExchange();

            _IAPManager.ON_PRODUCT_PURCHASED += OnProductPurchased;
        }

        protected override void Hide()
        {
            _IAPManager.ON_PRODUCT_PURCHASED -= OnProductPurchased;

            foreach (var product in _productMap.Values)
            {
                product.ON_CLICK -= OnProductClick;
            }
            _productMap.Clear();
        }

        private void SetProductsIAP()
        {
            foreach (var product in _view.ProductsIAP)
            {
                var productID = product.Config.ID;
                if (!_config.ShopProductsIAPMap.ContainsKey(productID))
                {
                    Log.Warning("Product " + productID + " not added to the GameConfig");
                    continue;
                }

                var price = _IAPManager.GetPrice(productID);
                var amount = product.Config.Reward.Amount;
                var amountResult = amount.ToString();

                _productMap.Add(productID, product);

                product.Initialize(price, amountResult);
                product.ON_CLICK += OnProductClick;
            }
        }

        private void SetProductsExchange()
        {
            foreach (var product in _view.ProductsExchange)
            {
                var config = product.Config as ShopProductExchangeConfig;
                var productID = config.ID;

                var priceAmount = config.Price.Amount;
                var priceLabel = GetPriceLabel(config.Price.ResourceType);

                var priceResult = string.Format(_priceFormat, priceLabel, priceAmount);
                var amountResult = config.Reward.Amount.ToString();

                _productMap.Add(productID, product);

                product.Initialize(priceResult, amountResult);
                product.ON_CLICK += OnProductClick;
            }
        }

        private string GetPriceLabel(ResourceItemType type)
        {
            switch (type)
            {
                case ResourceItemType.Cash:
                    return "CASH";
                case ResourceItemType.GemsPink:
                    return "GEMS";
                case ResourceItemType.Energy:
                    return "ENERGY";
                default:
                    return type.ToString().ToUpperInvariant();
            }
        }

        private void OnProductClick(string productID)
        {
            var product = _productMap[productID];
            var type = product.Config.ProductType;

            if(type == ShopProductType.ResourcesForRealMoney)
                _IAPManager.OnPurchaseClicked(productID);
            else if(type == ShopProductType.ResourcesExchange)
                TryExchange(product);
        }

        private void TryExchange(ShopProductView product)
        {
            var config = product.Config as ShopProductExchangeConfig;

            var priceType = config.Price.ResourceType;
            var priceAmount = config.Price.Amount;

            var rewardType = config.Reward.ResourceType;
            var rewardAmount = config.Reward.Amount;

            if (priceType == ResourceItemType.GemsPink && _menuManager.Model.Gems >= priceAmount)
            {
                _menuManager.Model.Gems -= priceAmount;
                _menuManager.Model.SaveResource(rewardType, rewardAmount);
            }
            else if(priceType == ResourceItemType.Cash && _menuManager.Model.Cash >= priceAmount)
            {
                _menuManager.Model.Cash -= priceAmount;
                _menuManager.Model.SaveResource(rewardType, rewardAmount);
            }
        }

        private void OnProductPurchased(string productID)
        {
            var product = _productMap[productID];

            var rewardType = product.Config.Reward.ResourceType;
            var rewardAmount = product.Config.Reward.Amount;

            _menuManager.Model.SaveResource(rewardType, rewardAmount);
        }
    }
}
