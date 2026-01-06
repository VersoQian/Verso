using Game.Core.UI;
using Game.Managers;
using Game.States;
using Injection;

namespace Game.UI.Hud
{
    public class LeaveGameplayHudMediator : Mediator<LeaveGameplayHudView>
    {
        [Inject] private HudManager _hudManager;
        [Inject] private GameStateManager _gameStateManager;

        protected override void Show()
        {
            _view.LeaveButton.onClick.AddListener(OnLeaveButtonClick);
            _view.ContinueButton.onClick.AddListener(OnContinueButtonClick);
        }

        protected override void Hide()
        {
            _view.LeaveButton.onClick.RemoveListener(OnLeaveButtonClick);
            _view.ContinueButton.onClick.RemoveListener(OnContinueButtonClick);
        }

        private void OnLeaveButtonClick()
        {
            _hudManager.HideAdditional<LeaveGameplayHudMediator>();
            _hudManager.HideAdditional<PauseHudMediator>();

            _gameStateManager.SwitchToState(new GameMenuState());
        }

        private void OnContinueButtonClick()
        {
            _hudManager.HideAdditional<LeaveGameplayHudMediator>();
            _hudManager.HideAdditional<PauseHudMediator>();
        }
    }
}
