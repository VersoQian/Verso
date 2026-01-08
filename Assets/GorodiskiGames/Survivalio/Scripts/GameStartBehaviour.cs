using Game.Core;
using Game.Managers;
using Game.States;
using Injection;
using UnityEngine;

namespace Game
{
    public sealed class GameStartBehaviour : MonoBehaviour
    {
        private Timer _timer;
        public Context Context { get; private set; }

        private void Start()
        {
            Debug.Log("[GameStartBehaviour] Start() called - Initializing timer...");
            _timer = new Timer();
            Debug.Log("[GameStartBehaviour] Timer initialized");

            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            Application.runInBackground = true;

            Debug.Log("[GameStartBehaviour] Creating Context...");
            var context = new Context();

            Debug.Log("[GameStartBehaviour] Installing managers...");
            context.Install(
                new Injector(context),
                new HudManager(),
                new GameStateManager(),
                new VibrationManager(),
                new ResourcesManager(),
                new iOSAuthorizationTrackingManager(),
                new IAPManager()
            );

            Debug.Log("[GameStartBehaviour] Installing components...");
            context.Install(GetComponents<Component>());
            context.Install(_timer);
            context.ApplyInstall();

            Debug.Log("[GameStartBehaviour] Switching to tracking state...");
            context.Get<GameStateManager>().SwitchToState(new GameTrackingTransparencyState());

            Context = context;
            Debug.Log("[GameStartBehaviour] Start() completed successfully");
        }

        public void Reload()
        {
            Context.Get<GameStateManager>().Dispose();
            Context.Dispose();
            Start();
        }

        private void Update()
        {
            if (_timer == null)
            {
                Debug.LogError("[GameStartBehaviour] _timer is null in Update! Start() may not have been called.");
                return;
            }
            _timer.Update();
        }

        private void LateUpdate()
        {
            if (_timer == null) return;
            _timer.LateUpdate();
        }

        private void FixedUpdate()
        {
            if (_timer == null) return;
            _timer.FixedUpdate();
        }
    }
}
