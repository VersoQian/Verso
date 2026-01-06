using Game.Core.UI;
using Game.Managers;
using Injection;

namespace Game.UI.Hud
{
    public sealed class SettingsHudMediator : Mediator<SettingsHudView>
    {
        [Inject] private HudManager _hudManager;
        [Inject] private MenuManager _menuManager;
        [Inject] private AudioManager _audioManager;

        protected override void Show()
        {
            var isDebugBuild = GameConstants.IsDebugBuild();
            _view.CheatPanelButton.gameObject.SetActive(isDebugBuild);

            _view.MusicVolumeSlider.Slider.value = _menuManager.Model.MusicVolume;
            _view.SFXVolumeSlider.Slider.value = _menuManager.Model.SFXVolume;

            _view.CheatPanelButton.onClick.AddListener(OnCheatPanelButtonClick);
            _view.CloseButton.onClick.AddListener(OnCloseButtonClick);
            _view.MusicVolumeSlider.Slider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
            _view.SFXVolumeSlider.Slider.onValueChanged.AddListener(OnSFXVolumeSliderChanged);
        }

        protected override void Hide()
        {
            _view.CheatPanelButton.onClick.RemoveListener(OnCheatPanelButtonClick);
            _view.CloseButton.onClick.RemoveListener(OnCloseButtonClick);
            _view.MusicVolumeSlider.Slider.onValueChanged.RemoveListener(OnMusicVolumeSliderChanged);
            _view.SFXVolumeSlider.Slider.onValueChanged.RemoveListener(OnSFXVolumeSliderChanged);
        }

        private void OnCloseButtonClick()
        {
            _hudManager.HideSingle();
        }

        private void OnCheatPanelButtonClick()
        {
            _hudManager.ShowAdditional<CheatPanelHudMediator>();
        }

        private void OnMusicVolumeSliderChanged(float value)
        {
            _audioManager.MusicVolume = value;
            _menuManager.Model.MusicVolume = value;
            _menuManager.Model.Save();

            _audioManager.FireMusicVolumeChanged(value);
        }

        private void OnSFXVolumeSliderChanged(float value)
        {
            _audioManager.SFXVolume = value;
            _menuManager.Model.SFXVolume = value;
            _menuManager.Model.Save();
        }
    }
}

