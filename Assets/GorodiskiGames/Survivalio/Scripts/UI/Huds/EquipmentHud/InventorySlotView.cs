using System;
using Game.Config;
using Game.Equipment;
using Game.Inventory;
using Game.Resource;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class InventorySlotView : BaseHudWithModel<InventoryModel>
    {
        public event Action<InventoryModel> ON_CLICK;

        private const string _levelFormat = "LVL {0}";
        private const string _amountFormat = "{0}";

        [SerializeField] private RectTransform _rectTransform;
        [SerializeField] private Button _button;
        [SerializeField] private Sprite _slotGreyIcon;
        [SerializeField] private Sprite _slotGreenIcon;
        [SerializeField] private Sprite _slotOrangeIcon;
        [SerializeField] private Image _slotIcon;
        [SerializeField] private Image _equipmentIcon;
        [SerializeField] private TMP_Text _infoText;

        private InventoryCategory _category;

        protected override void OnEnable()
        {
            _button.onClick.AddListener(OnClick);
        }

        protected override void OnDisable()
        {
            _button.onClick.RemoveListener(OnClick);
        }

        protected override void OnApplyModel(InventoryModel model)
        {
            base.OnApplyModel(model);

            if(model == null)
                return;

            var slotIcon = _slotGreyIcon;
            _category = model.Category;
            if (_category == InventoryCategory.Weapon)
                slotIcon = _slotGreenIcon;
            else if(_category == InventoryCategory.Resource)
                slotIcon = _slotOrangeIcon;

            _slotIcon.sprite = slotIcon;
        }

        private void OnClick()
        {
            ON_CLICK?.Invoke(Model);
        }

        public void SetParent(RectTransform rectTransform)
        {
            _rectTransform.SetParent(rectTransform);
            _rectTransform.localPosition = Vector3.zero;
            _rectTransform.localScale = Vector3.one;
        }

        protected override void OnModelChanged(InventoryModel model)
        {
            _equipmentIcon.sprite = model.Icon;

            // 根据装备配置添加颜色着色（区分Red/Grey/Orange变体）
            if (_category == InventoryCategory.Cloth)
            {
                var equipmentModel = model as EquipmentModel;
                var configName = equipmentModel.Config.name;

                // 根据配置名称判断颜色变体
                if (configName.Contains("Red") || configName.Contains("Default"))
                {
                    _equipmentIcon.color = new Color(1f, 0.3f, 0.3f); // 红色调
                }
                else if (configName.Contains("Grey"))
                {
                    _equipmentIcon.color = new Color(0.6f, 0.6f, 0.6f); // 灰色调
                }
                else if (configName.Contains("Orange"))
                {
                    _equipmentIcon.color = new Color(1f, 0.6f, 0.2f); // 橙色调
                }
                else
                {
                    _equipmentIcon.color = Color.white; // 默认白色
                }
            }
            else
            {
                _equipmentIcon.color = Color.white; // 武器和资源保持白色
            }

            var levelResult = string.Empty;
            if(_category == InventoryCategory.Cloth || _category == InventoryCategory.Weapon)
            {
                var equipmentModel = model as EquipmentModel;
                var lvlNice = equipmentModel.Level + 1;
                levelResult = string.Format(_levelFormat, lvlNice);
            }
            else if(_category == InventoryCategory.Resource)
            {
                var resourceModel = model as ResourceModel;
                var amount = resourceModel.Amount;
                levelResult = string.Format(_amountFormat, amount);
            }
            _infoText.text = levelResult;
        }
    }
}

