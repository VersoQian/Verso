using Game.Ability;
using Game.Managers;
using Game.Skill;
using Game.Weapon;
using Injection;
using System.Collections.Generic;
using System.Linq;

namespace Game.UI.Hud
{
    public sealed class SkillSelectionHudMediator : MediatorWithSelectables<SkillSelectionHudView>
    {
        [Inject] private LevelManager _levelManager;
        [Inject] private HudManager _hudManager;
        [Inject] private GameManager _gameManager;

        private readonly List<SkillModel> _skillsMap;

        public SkillSelectionHudMediator()
        {
            _skillsMap = new List<SkillModel>();
        }

        protected override void Show()
        {
            base.Show();

            _levelManager.Pause();

            var weaponConfigs = _levelManager.Model.WeaponConfigs;
            var slotIndex = 0;
            foreach (var config in weaponConfigs)
            {
                var serial = GameConstants.GetSerial();
                var index = config.Index;
                var existedWeapon = _levelManager.WeaponsMap.Keys.ToList().Find(w => w.Model.Index == index);
                var model = new WeaponModel(config, serial, 0);

                var isExisting = existedWeapon != null;
                if (isExisting)
                {
                    var count = existedWeapon.Model.Count;
                    count++;
                    model.Count = count;
                }

                var slot = _view.Slots.ToList().ElementAtOrDefault(slotIndex);
                if(slot == null)
                    continue;

                var type = SkillType.Weapon;
                slot.Initialize(type, model.Icon, model.Label, model.Count);

                _skillsMap.Add(model);

                slotIndex++;
            }

            var abilityConfigs = _levelManager.Model.AbilityConfigs;
            foreach (var config in abilityConfigs)
            {
                var type = config.Type;
                var existedAbility = _levelManager.Abilities.Find(a => a.Model.Type == type);
                var model = new AbilityModel(config);

                var isExisting = existedAbility != null;
                if (isExisting)
                {
                    var count = existedAbility.Model.Count;
                    count++;
                    model.Count = count;
                }

                var slot = _view.Slots.ToList().ElementAtOrDefault(slotIndex);
                if (slot == null)
                    continue;

                var skillType = SkillType.Ability;
                slot.Initialize(skillType, model.Icon, model.Label, model.Count);

                _skillsMap.Add(model);

                slotIndex++;
            }

            foreach (var icon in _view.WeaponIcons)
            {
                var index = _view.WeaponIcons.ToList().IndexOf(icon);
                var weapon = _levelManager.WeaponsMap.Keys.ToList().ElementAtOrDefault(index);

                var visibility = weapon != null;
                icon.gameObject.SetActive(visibility);

                if (!visibility)
                    continue;

                icon.sprite = weapon.Model.Icon;
            }

            foreach (var icon in _view.AbilityIcons)
            {
                var index = _view.AbilityIcons.ToList().IndexOf(icon);
                var ability = _levelManager.Abilities.ToList().ElementAtOrDefault(index);

                var visibility = ability != null;
                icon.gameObject.SetActive(visibility);

                if (!visibility)
                    continue;

                icon.sprite = ability.Model.Icon;
            }
        }

        protected override void Hide()
        {
            base.Hide();

            _skillsMap.Clear();

            foreach (var icon in _view.WeaponIcons)
            {
                icon.sprite = null;
            }

            foreach (var icon in _view.AbilityIcons)
            {
                icon.sprite = null;
            }

            _levelManager.Unpause();
        }

        public override void HandleLeft()
        {
            SelectPrevious();
        }

        public override void HandleRight()
        {
            SelectNext();
        }

        private void HideHud()
        {
            _hudManager.HideSingle();
        }

        protected override void HandleSelectableClick(int index)
        {
            var model = _skillsMap.ElementAt(index);
            if (model is WeaponModel weaponModel)
                _gameManager.FireWeaponSelected(weaponModel);
            else if (model is AbilityModel abilityModel)
                _gameManager.FireAbilitySelected(abilityModel);

            HideHud();
        }
    }
}

