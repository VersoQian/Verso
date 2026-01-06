namespace Game.Player.States
{
    public class PlayerWinState : PlayerState
    {
        public override void Initialize()
        {
            _player.View.Idle();
        }

        public override void Dispose()
        {

        }
    }
}

