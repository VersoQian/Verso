using Game.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class PauseHudView : BaseHudWithModel<LevelModel>
    {
        [SerializeField] private TMP_Text _heartsText;
        [SerializeField] private TMP_Text _enemiesKilledText;
        [SerializeField] private TMP_Text _cashText;
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _continueButton;
        [SerializeField] private SkillPauseSlotView[] _weaponSlots;
        [SerializeField] private SkillPauseSlotView[] _abilitySlots;

        public Button HomeButton => _homeButton;
        public Button ContinueButton => _continueButton;
        public SkillPauseSlotView[] WeaponSlots => _weaponSlots;
        public SkillPauseSlotView[] AbilitySlots => _abilitySlots;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }

        protected override void OnModelChanged(LevelModel model)
        {
            _heartsText.text = model.Hearts.ToString();
            _enemiesKilledText.text = model.EnemiesKilled.ToString();
            _cashText.text = model.Cash.ToString();
        }
    }
}

