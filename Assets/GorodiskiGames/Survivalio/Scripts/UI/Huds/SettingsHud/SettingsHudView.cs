using UnityEngine;
using UnityEngine.UI;

namespace Game.UI.Hud
{
    public sealed class SettingsHudView : BaseHud
    {
        [SerializeField] private Button _cheatPanelButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _restorePurchasesButton;
        [SerializeField] private SliderSelectableView _musicVolumeSlider;
        [SerializeField] private SliderSelectableView _sfxVolumeSlider;

        public Button CheatPanelButton => _cheatPanelButton;
        public Button CloseButton => _closeButton;
        public Button RestorePurchasesButton => _restorePurchasesButton;
        public SliderSelectableView MusicVolumeSlider => _musicVolumeSlider;
        public SliderSelectableView SFXVolumeSlider => _sfxVolumeSlider;

        protected override void OnEnable()
        {

        }

        protected override void OnDisable()
        {

        }
    }
}


