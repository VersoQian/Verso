using Game.Core.UI;
using Game.Managers;
using Injection;

namespace Game.UI.Hud
{
    public class GamePlayHudMediator : Mediator<GamePlayHudView>
    {
        [Inject] private HudManager _hudManager;
        [Inject] private LevelManager _levelManager;

        protected override void Show()
        {
            _view.Model = _levelManager.Model;

            _view.PauseButton.onClick.AddListener(OnPauseButtonClick);
        }

        protected override void Hide()
        {
            _view.PauseButton.onClick.RemoveListener(OnPauseButtonClick);
        }

        private void OnPauseButtonClick()
        {
            _hudManager.ShowAdditional<PauseHudMediator>();
        }
    }
}

