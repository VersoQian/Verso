using Game.Core;
using Game.Core.UI;
using Injection;
using UnityEngine;
using Game.Managers;

namespace Game.UI.Hud
{
    public sealed class DamageBorderHudMediator : Mediator<DamageBorderHudView>
    {
        private const float _timeClose = .06f;
        private const float _opacity = .6f;

        [Inject] private GameManager _gameManager;
        [Inject] private Timer _timer;

        private float _startTime;
        private float _duration;
        private bool _firstStep;

        protected override void Show()
        {
            _view.IsActive = false;

            _gameManager.Player.ON_DAMAGE += OnPlayerDamage;
        }

        protected override void Hide()
        {
            _gameManager.Player.ON_DAMAGE -= OnPlayerDamage;

            _timer.POST_TICK -= OnPostTick;
        }

        private void OnPlayerDamage()
        {
            _startTime = Time.time;
            _duration = _startTime - Time.time + _timeClose;
            _firstStep = true;

            _timer.POST_TICK += OnPostTick;
        }
        
        private void OnPostTick()
        {
            _view.IsActive = true;

            var factor = (Time.time - _startTime) / _duration;
            if (_firstStep)
            {
                _view.SetColor(new Color(1, 0, 0, Mathf.Lerp(0, _opacity, factor)));
                if (factor >= 1)
                {
                    _firstStep = false;
                    _startTime = Time.time;
                    _duration = _startTime - Time.time + _timeClose;
                    factor = 0;
                }
            }
            else
            {
                _view.SetColor(new Color(1, 0, 0, Mathf.Lerp(_opacity, 0, factor)));
            }

            if (factor < 1)
                return;

            _timer.POST_TICK -= OnPostTick;
        }
    }
}