using System.Linq;
using Game.Core;
using Game.Managers;
using Game.Modules;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.Enemy.States
{
    public sealed class EnemyFollowPlayerState : EnemyState
    {
        [Inject] private Timer _timer;
        [Inject] private GameManager _gameManager;
        [Inject] private LevelManager _levelManager;

        private float _walkSpeed;
        private bool _isPause;
        private TeamIDType _teamID;

        public override void Initialize()
        {
            _walkSpeed = _enemy.Model.Speed;
            _teamID = _enemy.TeamID;

            _enemy.View.Walk();

            _levelManager.ON_PAUSE += OnPause;
            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _levelManager.ON_PAUSE -= OnPause;
            _timer.TICK -= OnTick;
        }

        private void OnTick()
        {
            if(_isPause)
                return;

            var direction = (_gameManager.Player.View.Position - _enemy.View.Position).normalized;
            direction.y = 0f;
            _enemy.View.Rotation = Quaternion.LookRotation(direction);
            _enemy.View.Position += _walkSpeed * Time.deltaTime * _enemy.View.RotateNode.forward;
            
            CheckCollisionBullets();
            CheckCollisionPlayer();
        }

        private void OnPause(bool value)
        {
            _isPause = value;
        }

        private void CheckCollisionBullets()
        {
            foreach (var bullet in _gameManager.Bullets.ToList())
            {
                var type = bullet.Model.Type;
                if(type == Config.BulletControllerType.Parabola)
                    continue;

                var bulletTeamID = bullet.Model.TeamID;
                if (bulletTeamID == _teamID)
                    continue;

                var currentTime = _timer.Time;
                var canCollideUnit = bullet.CanCollideUnit(_enemy, currentTime);
                if (!canCollideUnit)
                    continue;

                var aimPosition = _enemy.View.AimPosition;
                var radius = _enemy.View.Radius;

                var bulletPosition = bullet.View.Position;
                var bulletRadius = bullet.View.Radius;

                var isCollided = VectorExtensions.IsCollided(aimPosition, radius, bulletPosition, bulletRadius);
                if (!isCollided)
                    continue;

                bullet.AddUnit(_enemy, currentTime);
                bullet.FireCollideEnemy();

                //enemy damage
                var damage = bullet.Model.Damage;
                var direction = (bulletPosition - aimPosition).normalized;
                _enemy.TryToDamage(damage, direction);
                //end enemy damage
            }
        }

        private void CheckCollisionPlayer()
        {
            var enemyAimPosition = _enemy.View.AimPosition;
            var enemyRadius = _enemy.View.Radius;

            var playerAimPosition = _gameManager.Player.View.AimPosition;
            var playerRadius = _gameManager.Player.View.Radius;

            var isCollided = VectorExtensions.IsCollided(enemyAimPosition, enemyRadius, playerAimPosition, playerRadius);
            if (!isCollided)
                return;

            //enemy damage
            var damage = _levelManager.WeaponsMap.Keys.FirstOrDefault().Model.Damage;
            var directionToPlayer = (playerAimPosition - enemyAimPosition).normalized;
            _enemy.TryToDamage(damage, directionToPlayer);
            //end enemy damage

            //player damage
            var playerDamage = _enemy.Model.Damage;
            var directionToEnemy = (enemyAimPosition - playerAimPosition).normalized;
            _gameManager.Player.TryToDamage(playerDamage, directionToEnemy, _gameManager);
            //end player damage
        }
    }
}

