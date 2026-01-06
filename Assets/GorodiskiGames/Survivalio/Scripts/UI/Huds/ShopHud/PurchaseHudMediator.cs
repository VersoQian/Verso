using Game.Core;
using Game.Core.UI;
using Game.Managers;
using Injection;
using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class PurchaseHudMediator : Mediator<PurchaseHudView>
    {
        private const float _delay = 1f;

        private const string _purchaseProcessingWord = "purchase processing...";
        private const string _purchaseRestoringWord = "restoring purchases...";

        [Inject] private IAPManager _IAPManager;
        [Inject] private Timer _timer;

        private float _hideTime;

        protected override void Show()
        {
            _hideTime = float.MaxValue;

            _view.InfoText.text = "";
            _view.BackgroundImage.SetActive(false);

            _IAPManager.ON_PURCHASE_CLICKED += OnPurchaseClicked;
            _IAPManager.ON_PURCHASE_FAILED += OnPurchaseFailed;
            _IAPManager.ON_PURCHASE_PROCESS_COMPLETE += OnPurchaseProcessComplete;

            _IAPManager.ON_RESTORE_PURCHASES += OnRestorePurchases;
            _IAPManager.ON_RESTORE_PURCHASES_END += OnRestorePurchasesEnd;

            _timer.TICK += OnTICK;
        }

        protected override void Hide()
        {
            _view.BackgroundImage.SetActive(false);

            _IAPManager.ON_PURCHASE_CLICKED -= OnPurchaseClicked;
            _IAPManager.ON_PURCHASE_FAILED -= OnPurchaseFailed;
            _IAPManager.ON_PURCHASE_PROCESS_COMPLETE -= OnPurchaseProcessComplete;

            _IAPManager.ON_RESTORE_PURCHASES -= OnRestorePurchases;
            _IAPManager.ON_RESTORE_PURCHASES_END -= OnRestorePurchasesEnd;

            _timer.TICK -= OnTICK;
        }

        private void OnTICK()
        {
            if (Time.time < _hideTime) return;

            _hideTime = float.MaxValue;
            _view.InfoText.text = "";
            _view.BackgroundImage.SetActive(false);
        }

        private void OnPurchaseClicked()
        {
            _view.InfoText.text = _purchaseProcessingWord;
            _view.BackgroundImage.SetActive(true);
        }

        private void OnPurchaseFailed(string info)
        {
            _view.InfoText.text = info;
            _hideTime = Time.time + _delay;
        }

        private void OnPurchaseProcessComplete()
        {
            _hideTime = Time.time + _delay;
        }

        private void OnRestorePurchases()
        {
            _view.InfoText.text = _purchaseRestoringWord;
            _view.BackgroundImage.SetActive(true);
        }

        private void OnRestorePurchasesEnd(string info)
        {
            _view.InfoText.text = info;
            _hideTime = Time.time + _delay;
        }
    }
}