using Game.Core;
using Game.Core.UI;
using Game.Managers;
using Injection;

namespace Game.UI.Hud
{
    public sealed class BossIncomingHudMediator : Mediator<BossIncomingHudView>
    {
        private const float _delay = 2.5f;

        [Inject] private Timer _timer;
        [Inject] private GameManager _gameManager;

        private float _closeTime;

        protected override void Show()
        {
            _closeTime = _timer.Time + _delay;

            _view.InfoHolder.SetActive(true);
            _view.Model = _gameManager.Boss.Model;

            _timer.TICK += OnTick;
        }

        protected override void Hide()
        {
            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            if(_timer.Time < _closeTime)
                return;

            _timer.TICK -= OnTick;

            _view.InfoHolder.SetActive(false);
        }
    }
}

