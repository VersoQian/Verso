using Game.UI;
using Injection;

namespace Game.Collectible.States
{
    public sealed class CollectibleOnStartState : CollectibleState
    {
        [Inject] private GameView _gameView;

        public override void Initialize()
        {
            _gameView.Joystick.ON_INPUT += OnInput;
        }

        public override void Dispose()
        {
            _gameView.Joystick.ON_INPUT -= OnInput;
        }

        private void OnInput()
        {
            _collectible.Idle();
        }
    }
}

