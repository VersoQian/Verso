using Game.Config;
using Game.Core.UI;
using Game.Equipment;
using Game.Managers;
using Injection;

namespace Game.UI.Hud
{
    public sealed class EquipmentPreviewHudMediator : Mediator<EquipmentPreviewHudView>
    {
        [Inject] private HudManager _hudManager;
        [Inject] private MenuManager _menuManager;

        private readonly EquipmentModel _model;

        public EquipmentPreviewHudMediator(EquipmentModel model)
        {
            _model = model;
        }

        protected override void Show()
        {
            _view.Model = _model;

            _view.CloseButton.onClick.AddListener(OnCloseButtonClick);
            _view.EquipButton.onClick.AddListener(OnEquipButtonClick);
            _view.UpgradeButton.onClick.AddListener(OnUpgradeButtonClick);
        }

        protected override void Hide()
        {
            _view.CloseButton.onClick.RemoveListener(OnCloseButtonClick);
            _view.EquipButton.onClick.RemoveListener(OnEquipButtonClick);
            _view.UpgradeButton.onClick.RemoveListener(OnUpgradeButtonClick);
        }

        private void OnCloseButtonClick()
        {
            _hudManager.HideSingle();
        }

        private void OnEquipButtonClick()
        {
            _hudManager.HideSingle();
            _menuManager.FireEquip(_model);
        }

        private void OnUpgradeButtonClick()
        {
            var price = _model.Price;
            if(_menuManager.Model.Cash < price)
                return;

            var level = _model.Level;
            level++;

            _model.Level = level;
            _model.UpdateStats();
            _model.SetChanged();

            var serial = _model.Serial;
            var category = _model.Category;

            if(category == InventoryCategory.Weapon)
                _menuManager.Player.Model.WeaponLevels[serial] = level;
            else if(category == InventoryCategory.Cloth)
                _menuManager.Player.Model.ClothLevels[serial] = level;

            _menuManager.Player.Model.Save();

            _menuManager.Model.Cash -= price;
            _menuManager.Model.Save();
            _menuManager.Model.SetChanged();
        }
    }
}

