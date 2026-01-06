using Game.Core;
using Game.Config;
using Game.Managers;
using Injection;
using Game.Ability;
using System.Linq;
using System.Collections.Generic;
using System;

namespace Game.Modules
{
    public sealed class AbilitiesModule : Module
    {
        [Inject] private GameManager _gameManager;
        [Inject] private Timer _timer;
        [Inject] private LevelManager _levelManager;
        [Inject] private Injector _injector;

        private readonly Dictionary<AbilityType, Type> _abilitiesMap;

        public AbilitiesModule()
        {
            _abilitiesMap  = new Dictionary<AbilityType, Type>()
            {
                [AbilityType.Magnet] = typeof(MagnetAbilityController),
                [AbilityType.EnergyDrink] = typeof(EnergyDrinkAbilityController)
            };
        }

        public override void Initialize()
        {
            _gameManager.ON_ABILITY_SELECTED += OnAbilitySelected;
            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _gameManager.ON_ABILITY_SELECTED -= OnAbilitySelected;
            _timer.TICK -= OnTick;

            foreach (var ability in _levelManager.Abilities)
            {
                ability.Dispose();
            }
            _levelManager.Abilities.Clear();
        }

        private void OnTick()
        {
            foreach (var abiltiy in _levelManager.Abilities)
            {
                abiltiy.Proceed();
            }
        }

        private void OnAbilitySelected(AbilityModel model)
        {
            var type = model.Type;
            var existed = _levelManager.Abilities.ToList().Find(temp => temp.Model.Type == type);

            if (existed != null)
            {
                existed.Update();
                return;
            }

            if (_levelManager.Abilities.Count >= GameConstants.SkillsMax)
            {
                Log.Info("Abilities limit reached!");
                return;
            }

            var controller = _abilitiesMap[type];
            var ability = (AbilityController)Activator.CreateInstance(controller, new object[] { model, });
            _injector.Inject(ability);

            ability.Init();

            _levelManager.Abilities.Add(ability);
        }
    }
}

