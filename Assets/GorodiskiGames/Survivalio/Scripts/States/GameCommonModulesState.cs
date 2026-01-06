using Game.Config;
using Game.Domain;
using Game.Managers;
using Game.Modules;
using Injection;
using UnityEngine;

namespace Game.States
{
    public class GameCommonModulesState : GameModulesState
    {
        [Inject] private Context _context;
        [Inject] private GameConfig _config;
        [Inject] private GameStateManager _gameStateManager;

        private AudioManager _audioManager;

        public override void Initialize()
        {
            var model = GameModel.Load(_config);
            _audioManager = new AudioManager(model.MusicVolume, model.SFXVolume);

            _context.Install(_audioManager);
            _context.ApplyInstall();

            InitLevelModules();

            // 始终进入主菜单界面，不直接进入游戏
            _gameStateManager.SwitchToState(new GameMenuState());
        }

        public override void Dispose()
        {

        }

        private void InitLevelModules()
        {
            AddModule<AudioModule, AudioModuleView>(_gameView);
        }
    }
}

