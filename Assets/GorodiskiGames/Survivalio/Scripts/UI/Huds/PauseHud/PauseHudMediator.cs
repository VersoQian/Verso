using System.Linq;
using Game.Core.UI;
using Game.Managers;
using Injection;
using UnityEngine;

namespace Game.UI.Hud
{
    public sealed class PauseHudMediator : Mediator<PauseHudView>
    {
        [Inject] private LevelManager _levelManager;
        [Inject] private HudManager _hudManager;

        protected override void Show()
        {
            _levelManager.Pause();

            _view.Model = _levelManager.Model;

            foreach (var slot in _view.WeaponSlots)
            {
                var index = _view.WeaponSlots.ToList().IndexOf(slot);
                var weapon = _levelManager.WeaponsMap.Keys.ToList().ElementAtOrDefault(index);

                Sprite icon = null;
                var count = 0;

                var isExists = weapon != null;
                if (isExists)
                {
                    icon = weapon.Model.Icon;
                    count = weapon.Model.Count;
                }

                var model = new SkillPauseSlotModel(icon, count);
                slot.Model = model;
            }

            foreach (var slot in _view.AbilitySlots)
            {
                var index = _view.AbilitySlots.ToList().IndexOf(slot);
                var ability = _levelManager.Abilities.ElementAtOrDefault(index);

                Sprite icon = null;
                var count = 0;

                var isExists = ability != null;
                if (isExists)
                {
                    icon = ability.Model.Icon;
                    count = ability.Model.Count;
                }

                var model = new SkillPauseSlotModel(icon, count);
                slot.Model = model;
            }

            _view.HomeButton.onClick.AddListener(OnHomeButtonClick);
            _view.ContinueButton.onClick.AddListener(OnContinueButtonClick);
        }

        protected override void Hide()
        {
            _view.HomeButton.onClick.RemoveListener(OnHomeButtonClick);
            _view.ContinueButton.onClick.RemoveListener(OnContinueButtonClick);

            foreach (var slot in _view.WeaponSlots)
            {
                slot.Model = null;
            }

            foreach (var slot in _view.AbilitySlots)
            {
                slot.Model = null;
            }

            _levelManager.Unpause();
        }

        private void OnContinueButtonClick()
        {
            _hudManager.HideAdditional<PauseHudMediator>();
        }

        private void OnHomeButtonClick()
        {
            _hudManager.ShowAdditional<LeaveGameplayHudMediator>();
        }
    }
}

