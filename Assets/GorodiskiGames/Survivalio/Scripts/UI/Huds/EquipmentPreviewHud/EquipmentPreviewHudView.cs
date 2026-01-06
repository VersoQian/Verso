using System.Linq;
using Game.Cloth;
using Game.Config;
using Game.Equipment;
using Game.Player;
using Game.Weapon;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class EquipmentPreviewHudView : BaseHudWithModel<EquipmentModel>
    {
        private const float _anchorXCenter = 0.5f;
        private const float _anchorXRight = 0.275f;
        private const float _anchorXLeft = 0.725f;

        private const string _maxWord = "MAX";
        private const string _levelFormat = "LEVEL: {0}/{1}";
        private const string _levelFormatUpgradedMax = "LEVEL: {0}";
        private const string _upgradeFormat = "{0}{1}\nUPGRADE";

        private const string _attributeFormat = "{0}:   {1} > {2}";
        private const string _attributeFormatUpgradedMax = "{0}:   {1}";

        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _equipButton;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private TMP_Text _upgradeButtonText;
        [SerializeField] private TMP_Text _labelText;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Image _levelImage;
        [SerializeField] private AttributeSlotView[] _playerAttributes;
        [SerializeField] private TMP_Text[] _attributesText;

        public Button CloseButton => _closeButton;
        public Button EquipButton => _equipButton;
        public Button UpgradeButton => _upgradeButton;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(EquipmentModel model)
        {
            var equipButtonVisibility = !model.IsEquipped;
            _equipButton.gameObject.SetActive(equipButtonVisibility);

            var upgradeButtonVisibility = !model.IsUpgradedMax;
            _upgradeButton.gameObject.SetActive(upgradeButtonVisibility);

            //upgrade button position
            var rectUpgradeButton = _upgradeButton.transform as RectTransform;
            var anchorUpgradeButton = equipButtonVisibility ? _anchorXLeft : _anchorXCenter;

            rectUpgradeButton.anchorMin = new Vector2(anchorUpgradeButton, rectUpgradeButton.anchorMin.y);
            rectUpgradeButton.anchorMax = new Vector2(anchorUpgradeButton, rectUpgradeButton.anchorMax.y);
            //upgrade button position end

            //equip button position
            var rectEquipButton = _equipButton.transform as RectTransform;
            var anchorEquipButton = upgradeButtonVisibility ? _anchorXRight : _anchorXCenter;

            rectEquipButton.anchorMin = new Vector2(anchorEquipButton, rectEquipButton.anchorMin.y);
            rectEquipButton.anchorMax = new Vector2(anchorEquipButton, rectEquipButton.anchorMax.y);
            //equip button position end

            _labelText.text = model.Label;
            _icon.sprite = model.Icon;
            _upgradeButtonText.text = string.Format(_upgradeFormat, GameConstants.CashIcon, model.Price);

            var levelNice = model.Level + 1;
            var levelText = string.Format(_levelFormat, levelNice, model.LevelsCount);
            if (model.IsUpgradedMax)
                levelText = string.Format(_levelFormatUpgradedMax, ColorUtil.ColorString(_maxWord, ColorUtil.HEXToColor(GameConstants.GreenColorHex)));

            _levelText.text = levelText;
            _levelImage.fillAmount = (float)levelNice / model.LevelsCount;

            SetAttribute(model);

            var category = model.Category;
            if(category == InventoryCategory.Weapon)
            {
                var weaponModel = model as WeaponModel;
                var value = weaponModel.Attack;

                SetPlayerAtribute(UnitAttributeType.Attack, value);
            }
            else if(category == InventoryCategory.Cloth)
            {
                var clothModel = model as ClothModel;
                var value = clothModel.Armor;
                SetPlayerAtribute(UnitAttributeType.Health, value);
            }
        }

        private void SetPlayerAtribute(UnitAttributeType attributeType, float value)
        {
            foreach (var attributeSlot in _playerAttributes)
            {
                var visibility = attributeSlot.Type == attributeType;
                attributeSlot.gameObject.SetActive(visibility);

                if (!visibility)
                    continue;

                attributeSlot.SetValue(value);
            }
        }

        private void SetAttribute(EquipmentModel model)
        {
            var attributes = model.Attributes;
            var attributesNext = model.AttributesNext;
            foreach (var type in attributes.Keys)
            {
                var index = attributes.Keys.ToList().IndexOf(type);

                var value = attributes[type].ToString();
                value = ColorUtil.ColorString(value, ColorUtil.HEXToColor(GameConstants.YellowColorHex));

                var valueNext = attributesNext[type].ToString();
                valueNext = ColorUtil.ColorString(valueNext, ColorUtil.HEXToColor(GameConstants.GreenColorHex));

                var attributeFormat = model.IsUpgradedMax ? _attributeFormatUpgradedMax : _attributeFormat;
                _attributesText[index].text = string.Format(attributeFormat, type, value, valueNext);
            }
        }
    }
}

