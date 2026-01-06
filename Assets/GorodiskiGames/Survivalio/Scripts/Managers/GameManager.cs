using System;
using System.Collections.Generic;
using Game.Bullet;
using Game.Config;
using Game.Enemy;
using Game.Effect;
using Game.UI;
using Game.Weapon;
using Injection;
using UnityEngine;
using Game.Modules;
using Game.Ability;

namespace Game.Managers
{
    public sealed class GameManager : BaseManager, IDisposable
    {
        private const float _minRange = 4f;

        public event Action<EnemyController> ON_ENEMY_DIED; //used in the CollectiblesModule to spawn reward
        public event Action<WeaponModel> ON_WEAPON_SELECTED;
        public event Action<AbilityModel> ON_ABILITY_SELECTED;
        public event Action<EffectType, Vector3, Vector3> ON_SPAWN_EFFECT;
        public event Action<Vector3, float> ON_SPAWN_EXPLOSION;

        public event Action<string, Vector3, UINotificationColorType> ON_SPAWN_NOTIFICATION_POPUP;

        [Inject] private GameView _gameView;

        public List<EnemyController> Enemies;
        private List<EnemyController> _enemiesCached;
        public EnemyController Boss;
        public readonly List<BulletController> Bullets;

        public GameManager(GameConfig config) : base(config)
        {
            Bullets = new List<BulletController>();
            Enemies = new List<EnemyController>();
            _enemiesCached = new List<EnemyController>();
        }

        public void Dispose()
        {
            _enemiesCached.Clear();
            Player.Dispose();
        }

        public EnemyController FindClosestEnemy(float range)
        {
            var shortestDistance = float.MaxValue;
            EnemyController result = null;

            foreach (var enemy in _enemiesCached)
            {
                var enemyPosition = enemy.View.Position;

                var isOnScreen = IsOnScreen(enemyPosition);
                if(!isOnScreen)
                    continue;

                var distance = Vector3.Distance(Player.View.Position, enemyPosition);
                if (distance < shortestDistance && distance <= range && distance > _minRange)
                {
                    shortestDistance = distance;
                    result = enemy;
                }
            }

            if(result != null && !result.IsBoss)
                _enemiesCached.Remove(result);

            return result;
        }

        private bool IsOnScreen(Vector3 pointInScene)
        {
            Vector2 pointOnScreen = _gameView.Camera.Camera.WorldToScreenPoint(pointInScene);

            if ((pointOnScreen.x < 0) || (pointOnScreen.x > Screen.width) ||
            (pointOnScreen.y < 0) || (pointOnScreen.y > Screen.height))
            {
                return false;
            }
            return true;
        }

        public void RefillCachedEnemies()
        {
            _enemiesCached.Clear();
            foreach (var enemy in Enemies)
            {
                _enemiesCached.Add(enemy);
            }
        }

        public void FireEnemyDied(EnemyController enemy)
        {
            ON_ENEMY_DIED?.Invoke(enemy);
        }

        public void FireWeaponSelected(WeaponModel model)
        {
            ON_WEAPON_SELECTED?.Invoke(model);
        }

        public void FireAbilitySelected(AbilityModel model)
        {
            ON_ABILITY_SELECTED?.Invoke(model);
        }

        public void FireSpawnEffect(EffectType type, Vector3 position, Vector3 direction)
        {
            ON_SPAWN_EFFECT?.Invoke(type, position, direction);
        }

        public void FireSpawnNotificationPopUp(string info, Vector3 position, UINotificationColorType colorType)
        {
            ON_SPAWN_NOTIFICATION_POPUP?.Invoke(info, position, colorType);
        }

        public void FireSpawnExplosion(Vector3 position, float damage)
        {
            ON_SPAWN_EXPLOSION?.Invoke(position, damage);
        }

        public void AddEnemy(EnemyController enemy)
        {
            Enemies.Add(enemy);
            _enemiesCached.Add(enemy);
        }

        public void RemoveEnemy(EnemyController enemy)
        {
            Enemies.Remove(enemy);
            _enemiesCached.Remove(enemy);
        }
    }
}

