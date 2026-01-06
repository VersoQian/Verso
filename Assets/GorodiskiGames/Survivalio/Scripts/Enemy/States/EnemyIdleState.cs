namespace Game.Enemy.States
{
    public sealed class EnemyIdleState : EnemyState
    {
        public override void Initialize()
        {
            _enemy.View.Idle();
        }

        public override void Dispose()
        {

        }
    }
}