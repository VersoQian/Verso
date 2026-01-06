using System.Linq;
using Game.Core;
using Game.Managers;
using Game.Modules;
using Injection;
using Utilities;

namespace Game.Player.States
{
    public class PlayerCheckCollisionState : PlayerState
    {
        [Inject] protected GameManager _gameManager;
        [Inject] protected LevelManager _levelManager;
        [Inject] protected Timer _timer;

        private TeamIDType _teamID;
        protected bool _isPause;

        public override void Initialize()
        {
            _teamID = _player.TeamID;

            _levelManager.ON_PAUSE += OnPause;
        }

        public override void Dispose()
        {
            _levelManager.ON_PAUSE -= OnPause;
        }

        public virtual void OnPause(bool value)
        {
            _isPause = value;
        }

        protected void CheckCollisionBullets()
        {
            foreach (var bullet in _gameManager.Bullets.ToList())
            {
                var bulletTeamID = bullet.Model.TeamID;
                if (bulletTeamID == _teamID)
                    continue;

                var currentTime = _timer.Time;
                var canCollideUnit = bullet.CanCollideUnit(_player, currentTime);
                if (!canCollideUnit)
                    continue;

                var aimPosition = _player.View.AimPosition;
                var radius = _player.View.Radius;

                var bulletPosition = bullet.View.Position;
                var bulletRadius = bullet.View.Radius;

                var isCollided = VectorExtensions.IsCollided(aimPosition, radius, bulletPosition, bulletRadius);
                if (!isCollided)
                    continue;

                bullet.AddUnit(_player, currentTime);
                bullet.FireCollideEnemy();

                //player damage
                var damage = bullet.Model.Damage;
                var direction = (bulletPosition - aimPosition).normalized;
                _player.TryToDamage(damage, direction, _gameManager);
                //end player damage
            }
        }
    }
}

