using System.Collections.Generic;
using Game.Cloth;
using Game.Config;
using Game.Equipment;
using Game.Inventory;
using Game.Managers;
using Game.Player;
using Game.Weapon;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class EquipmentHudMediator : RawCameraHudMediator<EquipmentHudView>
    {
        private const float _rotationY = 205f;
        private const float _anchorPositionY = 0.55f;

        [Inject] private GameConfig _config;
        [Inject] private MenuManager _menuManager;
        [Inject] private HudManager _hudManager;

        private PlayerController _player;

        private readonly Dictionary<EquipmentModel, InventorySlotView> _slotsMap;

        public EquipmentHudMediator()
        {
            _slotsMap = new Dictionary<EquipmentModel, InventorySlotView>();
        }

        protected override void Show()
        {
            SetViewFromCamera(_view.RawImage, out _rawCamera);

            _player = _menuManager.Player;
            _player.View.Position = _rawCamera.AnchorToWorldPosition(0.5f, _anchorPositionY);
            _player.View.Rotation = Quaternion.Euler(0f, _rotationY, 0f);
            _player.IdleMenu();

            var prefab = _resourcesManager.LoadInventorySlot();
            foreach (var serial in _player.Model.StoredWeapons.Keys)
            {
                var category = InventoryCategory.Weapon;
                var index = _player.Model.StoredWeapons[serial];
                var config = _config.WeaponMap[index];
                var level = _player.Model.WeaponLevels[serial];

                var model = _menuManager.CreateInventoryModel(category, config, index, serial, level);
                var weaponModel = model as WeaponModel;
                var slot = GameObject.Instantiate(prefab).GetComponent<InventorySlotView>();

                var parent = _view.Content;
                if (weaponModel.IsEquipped)
                    parent = _view.WeaponCell;
                slot.SetParent(parent);

                _slotsMap[weaponModel] = slot;

                slot.Model = model;
                slot.ON_CLICK += OnEquipmentSlotClick;
            }

            foreach (var serial in _player.Model.StoredCloth.Keys)
            {
                var category = InventoryCategory.Cloth;
                var index = _player.Model.StoredCloth[serial];
                var config = _config.ClothMap[index];
                var level = _player.Model.ClothLevels[serial];

                var model = _menuManager.CreateInventoryModel(category, config, index, serial, level);
                var clothModel = model as ClothModel;
                var slot = GameObject.Instantiate(prefab).GetComponent<InventorySlotView>();

                var parent = _view.Content;
                if(clothModel.IsEquipped)
                    parent = _view.ClothCellsMap[clothModel.ClothType];

                slot.SetParent(parent);

                _slotsMap[clothModel] = slot;

                slot.Model = model;
                slot.ON_CLICK += OnEquipmentSlotClick;
            }

            _view.Model = _player.Model;

            SetContentSize();

            _menuManager.ON_EQUIP += OnEquip;
        }

        protected override void Hide()
        {
            _menuManager.ON_EQUIP -= OnEquip;

            foreach (var slot in _slotsMap.Values)
            {
                slot.ON_CLICK -= OnEquipmentSlotClick;
                GameObject.Destroy(slot.gameObject);
            }
            _slotsMap.Clear();

            Object.Destroy(_rawCamera.gameObject);
        }

        private void OnEquip(EquipmentModel candidateModel)
        {
            var category = candidateModel.Category;
            foreach (var model in _slotsMap.Keys)
            {
                if (model.Category != category)
                    continue;

                if (!model.IsEquipped)
                    continue;

                if(category == InventoryCategory.Weapon)
                {
                    Unequip(model);
                    break;
                }
                else
                {
                    var clothModel = model as ClothModel;
                    var candidateClothModel = candidateModel as ClothModel;
                    if(clothModel.ClothType != candidateClothModel.ClothType)
                        continue;

                    Unequip(model);
                }
            }

            Equip(candidateModel);
        }

        private void Equip(EquipmentModel model)
        {
            model.IsEquipped = true;

            var slot = _slotsMap[model];
            var category = model.Category;
            var serial = model.Serial;

            RectTransform parent = null;
            if (category == InventoryCategory.Weapon)
            {
                var weaponModel = model as WeaponModel;

                var type = UnitAttributeType.Attack;
                var value = _player.Model.GetAttribute(type);
                value += weaponModel.Attack;

                _player.Model.SetAttribute(type, value);
                _player.Model.EquippedWeapon = serial;
                _player.Model.Save();
                _player.Model.SetChanged();

                parent = _view.WeaponCell;
            }
            else if(category == InventoryCategory.Cloth)
            {
                var clothModel = model as ClothModel;

                _player.Model.ClothMeshMap[clothModel.ClothType] = clothModel.Mesh;

                var type = UnitAttributeType.Health;
                var value = _player.Model.GetAttribute(type);
                value += clothModel.Armor;

                _player.Model.SetAttribute(type, value);
                _player.Model.UpdateNominalHealth(value);
                _player.Model.EquippedCloth.Add(serial);
                _player.Model.Save();
                _player.Model.SetChanged();

                var clothType = clothModel.ClothType;
                parent = _view.ClothCellsMap[clothType];

                _menuManager.Player.ChangeCloth();
            }

            slot.SetParent(parent);
            SetContentSize();
        }

        private void Unequip(EquipmentModel model)
        {
            var category = model.Category;
            if (category == InventoryCategory.Weapon)
            {
                var weaponModel = model as WeaponModel;

                var type = UnitAttributeType.Attack;
                var value = _player.Model.GetAttribute(type);
                value -= weaponModel.Attack;
                _player.Model.SetAttribute(type, value);

                _player.Model.EquippedWeapon = -1;
                _player.Model.Save();
                _player.Model.SetChanged();
            }
            else if(category == InventoryCategory.Cloth)
            {
                var clothModel = model as ClothModel;

                var type = UnitAttributeType.Health;
                var value = _player.Model.GetAttribute(type);
                value -= clothModel.Armor;

                _player.Model.SetAttribute(type, value);
                _player.Model.UpdateNominalHealth(value);

                var serial = model.Serial;
                _player.Model.EquippedCloth.Remove(serial);
                _player.Model.Save();
                _player.Model.SetChanged();
            }

            model.IsEquipped = false;

            var slot = _slotsMap[model];
            slot.SetParent(_view.Content);
            SetContentSize();
        }

        private void OnEquipmentSlotClick(InventoryModel model)
        {
            var category = model.Category;
            if(category == InventoryCategory.Resource)
                return;

            var equipmentModel = model as EquipmentModel;
            _hudManager.ShowSingle<EquipmentPreviewHudMediator>(equipmentModel);
        }

        private void SetContentSize()
        {
            var sizeResult = UIUtil.GetContentSize(_view.Content, _view.LayoutGroup);
            _view.Content.sizeDelta = sizeResult;
        }
    }
}

