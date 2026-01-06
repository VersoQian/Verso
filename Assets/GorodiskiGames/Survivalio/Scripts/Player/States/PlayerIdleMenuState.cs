namespace Game.Player.States
{
    public sealed class PlayerIdleMenuState : PlayerState
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

