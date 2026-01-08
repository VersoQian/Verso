using System;
using Game.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class ShopProductView : BaseHud
    {
        public Action<string> ON_CLICK;

        [SerializeField] private ShopProductConfig _config;
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _rewardText;
        [SerializeField] private TMP_Text _priceText;

        public TMP_Text PriceText => _priceText;
        public ShopProductConfig Config => _config;

        public void Initialize(string price, string reward)
        {
            // Ensure sprite tags render instead of showing raw text.
            if (_priceText != null)
                _priceText.richText = true;
            if (_rewardText != null)
                _rewardText.richText = true;

            _priceText.text = price;
            _rewardText.text = reward;

            Debug.Log($"[ShopProductView] Initialize - Price: {price}, Reward: {reward}");
        }

        protected override void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        protected override void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            ON_CLICK?.Invoke(_config.ID);
        }
    }
}
