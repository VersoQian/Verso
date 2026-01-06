using Game.Config;
using Game.Core.UI;
using Game.Managers;
using Injection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game.UI.Hud
{
    public sealed class CheatPanelHudMediator : Mediator<CheatPanelHudView>
    {
        private const string _addFormat = "{0} {1}";
        private const string _addWord = "ADD";
        private const string _removeWord = "REMOVE";

        private const int _energyAmount = 1;
        private const int _gemsAmount = 100;
        private const int _cashAmount = 10000;

        [Inject] private HudManager _hudManager;
        [Inject] private MenuManager _menuManager;

        protected override void Show()
        {
            _view.Model = _menuManager.Model;

            _view.AddEnergyButtonText.text = string.Format(_addFormat, _addWord, GameConstants.EnergyIcon);
            _view.RemoveEnergyButtonText.text = string.Format(_addFormat, _removeWord, GameConstants.EnergyIcon);
            _view.AddGemsButtonText.text = string.Format(_addFormat, _addWord, GameConstants.GemsIcon);
            _view.AddCashButtonText.text = string.Format(_addFormat, _addWord, GameConstants.CashIcon);

            _view.ResetButton.onClick.AddListener(OnResetButtonClick);
            _view.AddEnergyButton.onClick.AddListener(OnAddEnergyButtonClick);
            _view.RemoveEnergyButton.onClick.AddListener(OnRemoveEnergyButtonClick);
            _view.AddGemsButton.onClick.AddListener(OnAddGemsButtonClick);
            _view.AddCashButton.onClick.AddListener(OnAddCoinsButtonClick);
            _view.CloseButton.onClick.AddListener(OnCloseButtonClick);
        }

        protected override void Hide()
        {
            _view.Model = null;

            _view.ResetButton.onClick.RemoveListener(OnResetButtonClick);
            _view.AddEnergyButton.onClick.RemoveListener(OnAddEnergyButtonClick);
            _view.RemoveEnergyButton.onClick.RemoveListener(OnRemoveEnergyButtonClick);
            _view.AddGemsButton.onClick.RemoveListener(OnAddGemsButtonClick);
            _view.AddCashButton.onClick.RemoveListener(OnAddCoinsButtonClick);
            _view.CloseButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        private void OnResetButtonClick()
        {
            _menuManager.Model.Remove();
            SceneManager.LoadScene(0, LoadSceneMode.Single);
        }

        private void OnAddEnergyButtonClick()
        {
            _menuManager.Model.Energy += _energyAmount;
            _menuManager.Model.Save();
            _menuManager.Model.SetChanged();
        }

        private void OnRemoveEnergyButtonClick()
        {
            var energy = _menuManager.Model.Energy;
            if (energy == 0)
                return;
            
            _menuManager.Model.Energy = Mathf.Max(energy - _energyAmount, 0);
            _menuManager.Model.Save();
            _menuManager.Model.SetChanged();
        }

        private void OnAddGemsButtonClick()
        {
            _menuManager.Model.Gems += _gemsAmount;
            _menuManager.Model.Save();
            _menuManager.Model.SetChanged();
        }

        private void OnAddCoinsButtonClick()
        {
            _menuManager.Model.Cash += _cashAmount;
            _menuManager.Model.Save();
            _menuManager.Model.SetChanged();
        }

        private void OnCloseButtonClick()
        {
            _hudManager.HideAdditional<CheatPanelHudMediator>();
        }
    }
}

