using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class SkillSelectionHudView : HudWithSelectables
    {
        [SerializeField] private SkillSelectionSlotView[] _slots;
        [SerializeField] private Image[] _weaponIcons;
        [SerializeField] private Image[] _abilityIcons;

        public SkillSelectionSlotView[] Slots => _slots;
        public Image[] WeaponIcons => _weaponIcons;
        public Image[] AbilityIcons => _abilityIcons;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }
}

