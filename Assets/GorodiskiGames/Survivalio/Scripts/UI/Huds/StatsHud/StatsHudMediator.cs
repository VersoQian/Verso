using System;
using Core;
using Game.Config;
using Game.Core;
using Game.Core.UI;
using Game.Managers;
using Game.Utilities;
using Injection;
using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class StatsHudMediator : Mediator<StatsHudView>, IObserver
    {
        private const string _energyTimeTextFormat = "{0}:{1}";

        [Inject] private GameConfig _config;
        [Inject] private MenuManager _menuManager;
        [Inject] private Timer _timer;
        [Inject] private HudManager _hudManager;

        private int _energyRestoreInterval;
        private int _energyMax;
        private bool _isRestoreProcess;
        private DateTime _lastTime;

        protected override void Show()
        {
            _energyRestoreInterval = _config.EnergyRestoreInterval;
            _energyMax = _config.EnergyMax;
            _lastTime = DateUtils.LoadDate(GameConstants.EnergyRestoreLastDateKey);

            _view.GameStatsHudView.EnergyTimeText.text = string.Empty;
            _view.PlayerStatsHudView.Model = _menuManager.Player.Model;

            _view.GameStatsHudView.EnergyMax = _config.EnergyMax;
            _view.GameStatsHudView.Model = _menuManager.Model;

            CheckRestoreProcessOnStart();

            _view.GameStatsHudView.AddEnergyButton.onClick.AddListener(OnAddEnergyButtonClick);
            _view.GameStatsHudView.AddPassCardButton.onClick.AddListener(OnAddPassCardButtonClick);
            _view.GameStatsHudView.AddCreditButton.onClick.AddListener(OnAddCreditButtonClick);

            _menuManager.Model.AddObserver(this);
            _timer.ONE_SECOND_TICK += OnSecondTick;
        }

        protected override void Hide()
        {
            _view.GameStatsHudView.AddEnergyButton.onClick.RemoveListener(OnAddEnergyButtonClick);
            _view.GameStatsHudView.AddPassCardButton.onClick.RemoveListener(OnAddPassCardButtonClick);
            _view.GameStatsHudView.AddCreditButton.onClick.RemoveListener(OnAddCreditButtonClick);

            _menuManager.Model.RemoveObserver(this);
            _timer.ONE_SECOND_TICK -= OnSecondTick;
        }

        private void CheckRestoreProcessOnStart()
        {
            if (_menuManager.Model.Energy < _config.EnergyMax)
                _isRestoreProcess = true;
            else
                _isRestoreProcess = false;
        }

        public void OnObjectChanged(IObservable observable)
        {
            CheckRestoreProcess();
        }

        private void CheckRestoreProcess()
        {
            var model = _menuManager.Model;
            if (model.Energy < _config.EnergyMax && !_isRestoreProcess)
            {
                _lastTime = DateTime.Now;
                DateUtils.SaveDate(GameConstants.EnergyRestoreLastDateKey, DateTime.Now);

                _isRestoreProcess = true;
            }
            else if (model.Energy >= _config.EnergyMax && _isRestoreProcess)
            {
                _isRestoreProcess = false;
                _view.GameStatsHudView.EnergyTimeText.text = string.Empty;
            }
        }

        private void OnSecondTick()
        {
            if(_isRestoreProcess)
                RestoreEnergy();
        }

        private void RestoreEnergy()
        {
            TimeSpan elapsedTime = DateTime.Now - _lastTime;
            TimeSpan interval = TimeSpan.FromSeconds(_energyRestoreInterval);
            TimeSpan delta = interval - elapsedTime;

            var minutes = delta.Minutes;
            var seconds = delta.Seconds;

            _view.GameStatsHudView.EnergyTimeText.text = string.Format(_energyTimeTextFormat, minutes, seconds);

            if (elapsedTime.TotalSeconds >= _energyRestoreInterval)
            {
                _lastTime = DateTime.Now;
                DateUtils.SaveDate(GameConstants.EnergyRestoreLastDateKey, DateTime.Now);

                var energyToRestore = (int)(elapsedTime.TotalSeconds / _energyRestoreInterval);

                _menuManager.Model.Energy = Mathf.Min(_menuManager.Model.Energy + energyToRestore, _energyMax);
                _menuManager.Model.Save();
                _menuManager.Model.SetChanged();
            }
        }

        private void OnAddEnergyButtonClick()
        {
            _hudManager.ShowSingle<PurchaseEnergyHudMediator>();
        }

        private void OnAddPassCardButtonClick()
        {
            _menuManager.FireMenuButtonClick(MenuHudType.Shop);
        }

        private void OnAddCreditButtonClick()
        {
            _menuManager.FireMenuButtonClick(MenuHudType.Shop);
        }
    }
}

