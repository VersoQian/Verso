using Game.Managers;
using Injection;

namespace Game.Player.States
{
    public sealed class PlayerDieState : PlayerState
    {
        [Inject] private LevelManager _levelManager;

        public override void Initialize()
        {
            _player.View.SetCollider(false);
            _player.View.Die();

            _levelManager.FireLevelEnd(false);
        }

        public override void Dispose()
        {

        }
    }
}