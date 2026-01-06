using System;
using System.Collections.Generic;
using Game.Core;
using Game.UI;
using Injection;
using UnityEngine;

namespace Game.States
{
    public abstract class GameState : State
    {

    }

    public abstract class GameModulesState : GameState
    {
        [Inject] protected Injector _injector;
        [Inject] protected GameView _gameView;

        protected readonly List<Module> _modules;

        public GameModulesState()
        {
            _modules = new List<Module>();
        }

        protected void AddModule<T>(params object[] args) where T : Module
        {
            var instance = (T)Activator.CreateInstance(typeof(T), args);
            AddModule(instance);
        }

        protected void AddModule<T, TView>(MonoBehaviour moduleView) where T : Module
        {
            var view = moduleView.GetComponent<TView>();
            if (view == null)
                return;

            var instance = (T)Activator.CreateInstance(typeof(T), new object[] { view });
            AddModule(instance);
        }

        protected void AddModule(Module module)
        {
            _modules.Add(module);
            _injector.Inject(module);
            module.Initialize();
        }

        protected void DisposeModules()
        {
            foreach (var m in _modules)
                m.Dispose();

            _modules.Clear();
        }
    }
}