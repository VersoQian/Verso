using System.Collections.Generic;
using Game.Config;
using Game.Core.UI;
using Game.Inventory;
using Game.Managers;
using Game.Resource;
using Game.States;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.UI.Hud
{
    public sealed class DefeatHudMediator : Mediator<DefeatHudView>
    {
        [Inject] private GameStateManager _gameStateManager;
        [Inject] private HudManager _hudManager;
        [Inject] private LevelManager _levelManager;
        [Inject] private ResourcesManager _resourcesManager;
        [Inject] private GameConfig _config;
        [Inject] private GameManager _gameManager;

        private readonly List<InventoryModel> _inventories;
        private readonly List<GameObject> _slots;
        private GameObject _slotPrefab;

        public DefeatHudMediator()
        {
            _inventories = new List<InventoryModel>();
            _slots = new List<GameObject>();
        }

        protected override void Show()
        {
            _slotPrefab = _resourcesManager.LoadInventorySlot();

            foreach (var type in _levelManager.Model.CollectedResourcesMap.Keys)
            {
                if (!_config.ResourcesMap.TryGetValue(type, out ResourceConfig config))
                    continue;

                var amount = _levelManager.Model.CollectedResourcesMap[type];
                if (amount <= 0)
                    continue;

                var model = new ResourceModel(config, amount);

                CreateSlot(model);
            }

            SetContentSize();

            _view.Model = _levelManager.Model;

            _view.ConfirmButton.onClick.AddListener(OnConfirmButtonClick);
        }

        protected override void Hide()
        {
            _view.ConfirmButton.onClick.RemoveListener(OnConfirmButtonClick);

            foreach (var slot in _slots)
            {
                GameObject.Destroy(slot);
            }
            _slots.Clear();
            _inventories.Clear();
        }

        private void OnConfirmButtonClick()
        {
            _gameManager.SaveInventory(_inventories);
            _hudManager.HideSingle();
            _gameStateManager.SwitchToState(new GameMenuState());
        }

        private void SetContentSize()
        {
            var sizeResult = UIUtil.GetContentSize(_view.Content, _view.LayoutGroup);
            _view.Content.sizeDelta = sizeResult;
        }

        private void CreateSlot(InventoryModel model)
        {
            var slot = GameObject.Instantiate(_slotPrefab).GetComponent<InventorySlotView>();
            slot.Model = model;
            slot.SetParent(_view.Content);

            _slots.Add(slot.gameObject);
            _inventories.Add(model);
        }
    }
}