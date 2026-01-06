using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Config;
using Game.Core;
using Injection;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;

namespace Game.Managers
{
    public sealed class Receipt
    {
        public string Store;
        public string TransactionID;
        public string Payload;

        public Receipt()
        {
            Store = TransactionID = Payload = "";
        }

        public Receipt(string store, string transactionID, string payload)
        {
            Store = store;
            TransactionID = transactionID;
            Payload = payload;
        }
    }

    public sealed class PayloadAndroid
    {
        public string json;
        public string signature;

        public PayloadAndroid()
        {
            json = signature = "";
        }

        public PayloadAndroid(string _json, string _signature)
        {
            json = _json;
            signature = _signature;
        }
    }

    public sealed class IAPManager : IDetailedStoreListener
    {
        public event Action ON_PURCHASE_CLICKED;
        public event Action<string> ON_PURCHASE_FAILED;
        public event Action ON_PURCHASE_PROCESS_COMPLETE;
        public event Action ON_RESTORE_PURCHASES;
        public event Action<string> ON_RESTORE_PURCHASES_END;
        public event Action<string> ON_PRODUCT_PURCHASED;

        private const string kEnvironment = "production";

        private IStoreController _controller;
        private IExtensionProvider _extension;

        private readonly Dictionary<string, string> _productsPriceMap;

        public IAPManager()
        {
            _productsPriceMap = new Dictionary<string, string>();
        }

        async public void Initialize(GameConfig config)
        {
            try
            {
                var options = new InitializationOptions().SetEnvironmentName(kEnvironment);
                await UnityServices.InitializeAsync(options);
            }
            catch (Exception exception)
            {
                Log.Info(exception.Message);
            }

            InitializePurchasing(config);
        }

        public string GetPrice(string productID)
        {
            return _productsPriceMap.ContainsKey(productID) ? _productsPriceMap[productID] : 0.ToString();
        }

        public void OnPurchaseClicked(string productId)
        {
            if (IsPurchaseInitialized())
            {
                Product product = _controller.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    ON_PURCHASE_CLICKED?.Invoke();

                    Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
                    _controller.InitiatePurchase(product);
                }
                else
                {
                    Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
                }
            }
            else
            {
                Debug.Log("BuyProductID FAIL. Not initialized.");
            }
        }

        public void RestorePurchases()
        {
            if (!IsPurchaseInitialized())
            {
                Debug.Log("RestorePurchases FAIL. Not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                Debug.Log("RestorePurchases started ...");
                ON_RESTORE_PURCHASES?.Invoke();

                var apple = _extension.GetExtension<IAppleExtensions>();

                apple.RestoreTransactions((result, info) =>
                {
                    ON_RESTORE_PURCHASES_END?.Invoke(info);
                    Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
                });
            }
            else
            {
                Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
            }
        }

        private bool IsPurchaseInitialized()
        {
            return _controller != null && _extension != null;
        }

        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            Debug.Log("IAP. Initialize success!");

            _controller = controller;
            _extension = extensions;

            foreach (var product in _controller.products.all)
            {
                var id = product.definition.id;
                var price = product.metadata.localizedPriceString;

                _productsPriceMap.Add(id, price);

                if (!product.availableToPurchase)
                    Log.Info($"Product not available for purchase {product.definition.id}");
            }
        }

        private void InitializePurchasing(GameConfig config)
        {
            if (IsPurchaseInitialized())
            {
                return;
            }

            var purchasing = StandardPurchasingModule.Instance();
            var builder = ConfigurationBuilder.Instance(purchasing);

#if UNITY_EDITOR
            purchasing.useFakeStoreAlways = true;
            purchasing.useFakeStoreUIMode = FakeStoreUIMode.Default;
#endif

            foreach (var product in config.ShopProductsIAPMap.Values)
            {
                builder.AddProduct(product.ID, product.Type);
            }

            UnityPurchasing.Initialize(this, builder);
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            var id = args.purchasedProduct.definition.id;
            ON_PRODUCT_PURCHASED?.Invoke(id);

            ON_PURCHASE_PROCESS_COMPLETE?.Invoke();

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
        {
            string info = $"Purchase {product.metadata.localizedTitle} Failed. Reason: {failureReason}";
            ON_PURCHASE_FAILED.Invoke(info);
            Log.Info(info);
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
        {
            string info = $"Purchase {product.metadata.localizedTitle} Failed. Reason: {failureDescription}";
            ON_PURCHASE_FAILED.Invoke(info);
            Log.Info(info);
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.Log("Initialize failed due to: " + error);
        }

        public void OnInitializeFailed(InitializationFailureReason error, string message)
        {
            Debug.Log("Initialize failed due to: " + error);
        }
    }
}

