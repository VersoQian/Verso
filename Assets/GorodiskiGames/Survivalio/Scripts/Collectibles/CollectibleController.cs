using System;
using Core;
using Game.Core;
using Game.Collectible.States;
using Injection;
using UnityEngine;
using Game.Config;

namespace Game.Collectible
{
    public sealed class CollectibleController : IDisposable
    {
        public event Action<CollectibleController> ON_COLLECTED;

        private readonly CollectibleView _view;
        private readonly ResourceItemType _type;

        public CollectibleView View => _view;
        public ResourceItemType ResourceType => _type;

        private readonly StateManager<CollectibleState> _stateManager;

        public CollectibleController(CollectibleView view, ResourceItemType type, Context context)
        {
            _type = type;
            _view = view;

            var subContext = new Context(context);
            var injector = new Injector(subContext);

            subContext.Install(this);
            subContext.Install(injector);

            _stateManager = new StateManager<CollectibleState>();
            _stateManager.IsLogEnabled = false;

            injector.Inject(_stateManager);
        }

        public void Dispose()
        {
            _stateManager.Dispose();
        }

        public void OnStart()
        {
            _stateManager.SwitchToState(new CollectibleOnStartState());
        }

        public void Intro(Vector3 endPosition)
        {
            _stateManager.SwitchToState(new CollectibleIntroState(endPosition));
        }

        public void Idle()
        {
            _stateManager.SwitchToState(new CollectibleIdleState());
        }

        public void FlyToPlayer()
        {
            _stateManager.SwitchToState(new CollectibleFlyToPlayerState());
        }

        public void FireCollected()
        {
            ON_COLLECTED?.Invoke(this);
        }
    }
}

