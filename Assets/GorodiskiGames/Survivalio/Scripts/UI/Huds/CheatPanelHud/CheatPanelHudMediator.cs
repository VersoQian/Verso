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
        private const int _passCardAmount = 100;
        private const int _creditAmount = 10000;

        [Inject] private HudManager _hudManager;
        [Inject] private MenuManager _menuManager;

        protected override void Show()
        {
            _view.Model = _menuManager.Model;

            _view.AddEnergyButtonText.text = string.Format(_addFormat, _addWord, GameConstants.EnergyIcon);
            _view.RemoveEnergyButtonText.text = string.Format(_addFormat, _removeWord, GameConstants.EnergyIcon);
            _view.AddPassCardButtonText.text = string.Format(_addFormat, _addWord, GameConstants.PassCardIcon);
            _view.AddCreditButtonText.text = string.Format(_addFormat, _addWord, GameConstants.CreditIcon);

            _view.ResetButton.onClick.AddListener(OnResetButtonClick);
            _view.AddEnergyButton.onClick.AddListener(OnAddEnergyButtonClick);
            _view.RemoveEnergyButton.onClick.AddListener(OnRemoveEnergyButtonClick);
            _view.AddPassCardButton.onClick.AddListener(OnAddPassCardButtonClick);
            _view.AddCreditButton.onClick.AddListener(OnAddCreditButtonClick);
            _view.CloseButton.onClick.AddListener(OnCloseButtonClick);
        }

        protected override void Hide()
        {
            _view.Model = null;

            _view.ResetButton.onClick.RemoveListener(OnResetButtonClick);
            _view.AddEnergyButton.onClick.RemoveListener(OnAddEnergyButtonClick);
            _view.RemoveEnergyButton.onClick.RemoveListener(OnRemoveEnergyButtonClick);
            _view.AddPassCardButton.onClick.RemoveListener(OnAddPassCardButtonClick);
            _view.AddCreditButton.onClick.RemoveListener(OnAddCreditButtonClick);
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

        private void OnAddPassCardButtonClick()
        {
            _menuManager.Model.PassCard += _passCardAmount;
            _menuManager.Model.Save();
            _menuManager.Model.SetChanged();
        }

        private void OnAddCreditButtonClick()
        {
            _menuManager.Model.Credit += _creditAmount;
            _menuManager.Model.Save();
            _menuManager.Model.SetChanged();
        }

        private void OnCloseButtonClick()
        {
            _hudManager.HideAdditional<CheatPanelHudMediator>();
        }
    }
}

