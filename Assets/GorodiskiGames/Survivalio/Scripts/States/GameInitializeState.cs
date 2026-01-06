using System;
using Game.Config;
using Game.Domain;
using Game.Managers;
using Game.Utilities;
using Injection;
using UnityEngine;

namespace Game.States
{
    public sealed class GameInitializeState : GameState
    {
        [Inject] private GameStateManager _gameStateManager;
        [Inject] private IAPManager _IAPManager;
        [Inject] private VibrationManager _vibrationManager;
        [Inject] private GameConfig _config;

        private GameModel _model;

        public override void Initialize()
        {
            _model = GameModel.Load(_config);
            RestoreEnergyFromLastSession();

            _IAPManager.Initialize(_config);
            _vibrationManager.Initialize(_model.IsVibration);

            _gameStateManager.SwitchToState(new GameCommonModulesState());

            //var key = GameConstants.LoginToTheGameKey;
            //var loginToTheGame = PlayerPrefs.GetInt(key, 0);
            //if (loginToTheGame == 0)
            //{
            //    PlayerPrefs.SetInt(key, 1);
            //    PlayerPrefs.Save();

            //    _gameStateManager.SwitchToState(new GamePlayState());
            //}
            //else
            //{
            //    _gameStateManager.SwitchToState(new GameMenuState());
            //}

        }

        public override void Dispose()
        {
            _model = null;
        }

        private void RestoreEnergyFromLastSession()
        {
            if (_model.Energy >= _config.EnergyMax)
                return;

            var key = GameConstants.EnergyRestoreLastDateKey;
            var energyRestoreInterval = _config.EnergyRestoreInterval;

            DateTime lastTime = DateUtils.LoadDate(key);
            TimeSpan elapsedTime = DateTime.Now - lastTime;

            var energyToRestore = (int)(elapsedTime.TotalSeconds / energyRestoreInterval);
            _model.Energy = Mathf.Min(_model.Energy + energyToRestore, _config.EnergyMax);
            _model.Save();

            TimeSpan delta = TimeSpan.Zero;
            if (energyToRestore > 0)
            {
                var deltaSeconds = energyToRestore * energyRestoreInterval;
                delta = TimeSpan.FromSeconds(deltaSeconds);
            }

            DateTime lastTimeResult = lastTime.Add(delta);
            DateUtils.SaveDate(key, lastTimeResult);
        }
    }
}
