using System;
using Game.Config;
using Game.Domain;
using Game.Managers;
using Game.UI.Hud;
using Game.Utilities;
using Injection;
using UnityEngine;

namespace Game.States
{
    public sealed class GameTrackingTransparencyState : GameState
    {
        [Inject] private iOSAuthorizationTrackingManager _iOSAuthorizationTrackingManager;
        [Inject] private GameStateManager _gameStateManager;
        [Inject] private HudManager _hudManager;
        [Inject] private Context _context;

        public override void Initialize()
        {
            var config = GameConfig.Load();

            _context.Install(config);
            _context.ApplyInstall();

            _hudManager.ShowAdditional<SplashScreenHudMediator>(false);

#if UNITY_IOS && !UNITY_EDITOR
            _iOSAuthorizationTrackingManager.OnTrackingAuthorizationStatusReceived += OnTrackingAuthorizationStatusReceived;
            _iOSAuthorizationTrackingManager.Initialize();
#else
            Continue();
#endif
        }

        public override void Dispose()
        {
#if UNITY_IOS
            _iOSAuthorizationTrackingManager.OnTrackingAuthorizationStatusReceived -= OnTrackingAuthorizationStatusReceived;
#endif
            _hudManager.HideAdditional<SplashScreenHudMediator>();
        }

        private void OnTrackingAuthorizationStatusReceived(int status)
        {
#if UNITY_IOS
            _iOSAuthorizationTrackingManager.OnTrackingAuthorizationStatusReceived -= OnTrackingAuthorizationStatusReceived;
#endif
            Continue();
        }

        private void Continue()
        {
            _gameStateManager.SwitchToState(new GameInitializeState());
        }
    }
}