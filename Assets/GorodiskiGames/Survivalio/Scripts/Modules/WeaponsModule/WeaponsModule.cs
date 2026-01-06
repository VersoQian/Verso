using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Game.Bullet;
using Game.Config;
using Game.Core;
using Game.Enemy;
using Game.Managers;
using Game.UI;
using Game.Unit;
using Game.Weapon;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.Modules
{
    public sealed class WeaponsModule : Module<WeaponsModuleView>
    {
        [Inject] private GameConfig _config;
        [Inject] private GameManager _gameManager;
        [Inject] private GameView _gameView;
        [Inject] private Context _context;
        [Inject] private Timer _timer;
        [Inject] private LevelManager _levelManager;
        [Inject] private Injector _injector;

        private bool _isPause;
        private readonly Dictionary<BulletControllerType, Type> _bulletControllerMap;

        public WeaponsModule(WeaponsModuleView view): base(view)
        {
            _bulletControllerMap = new Dictionary<BulletControllerType, Type>
            {
                [BulletControllerType.Straight] = typeof(StraightBulletController),
                [BulletControllerType.RotateAround] = typeof(RotateAroundBulletController),
                [BulletControllerType.Return] = typeof(ReturnBulletController),
                [BulletControllerType.Parabola] = typeof(ParabolaBulletController)
            };
        }

        public override void Initialize()
        {
            //add selected weapon
            var player = _gameManager.Player;
            var serial = player.Model.EquippedWeapon;
            var index = 0;
            var level = 0;

            if(player.Model.StoredWeapons.ContainsKey(serial))
            {
                index = player.Model.StoredWeapons[serial];
                level = player.Model.WeaponLevels[serial];
            }

            var config = _config.WeaponMap[index];
            var model = new WeaponModel(config, serial, level);

            OnWeaponSelected(model);

            _gameView.WeaponView.Model = model;
            //end add selected weapon

            _levelManager.ON_PAUSE += OnPause;
            _levelManager.ON_LEVEL_END += OnLevelEnd;
            _levelManager.ON_BOSS_SPAWNED += OnBossSpawned;
            _gameManager.ON_WEAPON_SELECTED += OnWeaponSelected;
            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _levelManager.ON_PAUSE -= OnPause;
            _levelManager.ON_LEVEL_END -= OnLevelEnd;
            _levelManager.ON_BOSS_SPAWNED -= OnBossSpawned;
            _gameManager.ON_WEAPON_SELECTED -= OnWeaponSelected;
            _timer.TICK -= OnTick;

            foreach (var weapon in _levelManager.WeaponsMap.Keys)
            {
                weapon.ON_READY -= OnWeaponReady;
                weapon.Dispose();
            }

            foreach(var pool in _view.BulletsPool)
            {
                pool.ReleaseAllInstances();
            }

            foreach (var bullet in _gameManager.Bullets)
            {
                bullet.ON_COLLIDE_ENEMY -= RemoveBullet;
                bullet.ON_LIFETIME_END -= RemoveBullet;

                bullet.Dispose();
            }
            _gameManager.Bullets.Clear();
        }

        private void OnTick()
        {
            if(_isPause)
                return;

            foreach (var bullet in _gameManager.Bullets.ToList())
            {
                bullet.Proceed();

                if (!bullet.TimeIsUp())
                    continue;

                bullet.FireLifetimeEnd();
            }
        }

        private void OnLevelEnd(bool isWin)
        {
            _levelManager.ON_LEVEL_END -= OnLevelEnd;

            foreach (var weapon in _levelManager.WeaponsMap.Keys)
            {
                weapon.ON_READY -= OnWeaponReady;
                weapon.Dispose();
            }
        }

        private void OnPause(bool pause)
        {
            _isPause = pause;
        }

        private void OnWeaponSelected(WeaponModel model)
        {
            OnWeaponSelected(model, _gameManager.Player);
        }

        private void OnWeaponSelected(WeaponModel model, UnitController unit)
        {
            var index = model.Index;
            var weaponExisted = _levelManager.WeaponsMap.Keys.ToList().Find(w => w.Model.Index == index);

            if(weaponExisted != null)
            {
                var count = weaponExisted.Model.Count;
                count = Mathf.Min(count + 1, GameConstants.BulletsMax);
                weaponExisted.Model.Count = count;
                return;
            }

            if (_levelManager.WeaponsMap.Count >= GameConstants.SkillsMax)
            {
                Log.Warning("Weapon limit reached!");
                return;
            }

            var weapon = new WeaponController(model, _context);
            _levelManager.WeaponsMap[weapon] = unit;

            weapon.ON_READY += OnWeaponReady;
            weapon.Ready();
        }

        private void OnWeaponReady(WeaponController weapon)
        {
            if(_isPause)
                return;

            var delay = weapon.Model.MultipleBulletsDelay;
            _view.StartCoroutine(SpawnBulletsCoroutine(weapon, delay));

            var spawnBulletsTime = weapon.Model.Count * delay;
            weapon.Reload(spawnBulletsTime);
        }

        private IEnumerator SpawnBulletsCoroutine(WeaponController weapon, float delay)
        {
            var range = weapon.Model.Range;
            var bulletType = weapon.Model.BulletType;
            var controller = _bulletControllerMap[bulletType];
            var bulletIndex = weapon.Model.BulletIndex;
            var pool = _view.BulletsPool[bulletIndex];
            var unit = _levelManager.WeaponsMap[weapon];
            var count = weapon.Model.Count;

            for (int i = 0; i < count; i++)
            {
                var view = pool.Get<BulletView>();
                var moveSpeed = weapon.Model.BulletMoveSpeed;
                var lifeTime = weapon.Model.BulletLifeTime;
                var damage = weapon.Model.Damage;
                var orbitRadius = weapon.Model.OrbitRadius;

                var model = new BulletModel(bulletType, bulletIndex, i, count, moveSpeed, lifeTime, range, damage, orbitRadius, unit.TeamID);
                var bullet = (BulletController)Activator.CreateInstance(controller, new object[] { view, model, unit });
                _injector.Inject(bullet);

                var bulletPosition = unit.View.BulletNode.position;
                var aimPosition = GetAimPosition(unit, range, bulletType);
                bullet.Init(bulletPosition, aimPosition);

                bullet.ON_COLLIDE_ENEMY += RemoveBullet;
                bullet.ON_LIFETIME_END += RemoveBullet;

                _gameManager.Bullets.Add(bullet);

                yield return new WaitForSeconds(delay);
            }
        }

        private Vector3 GetAimPosition(UnitController unit, float range, BulletControllerType type)
        {
            var randomDirection = new Vector3(MathUtil.RandomFromOneToMinusOne(), 0f, MathUtil.RandomFromOneToMinusOne());
            var result = unit.View.Position + randomDirection * range;

            var isNeedUnitToAim = type == BulletControllerType.Straight || type == BulletControllerType.Return || type == BulletControllerType.Parabola;
            if(isNeedUnitToAim)
            {
                var teamID = unit.TeamID;
                if(teamID == TeamIDType.Player)
                {
                    var enemy = _gameManager.FindClosestEnemy(range);
                    if(enemy != null)
                        result = enemy.View.Position;
                }
                else if(teamID == TeamIDType.Enemy)
                    result = _gameManager.Player.View.Position;
            }

            return result;
        }

        private void RemoveBullet(BulletController bullet)
        {
            bullet.ON_COLLIDE_ENEMY -= RemoveBullet;
            bullet.ON_LIFETIME_END -= RemoveBullet;

            bullet.Dispose();

            _gameManager.Bullets.Remove(bullet);

            var bulletIndex = bullet.Model.Index;
            var pool = _view.BulletsPool[bulletIndex];

            pool.Release(bullet.View);
        }

        private void OnBossSpawned(EnemyController boss)
        {
            _levelManager.ON_BOSS_SPAWNED -= OnBossSpawned;

            var view = boss.View as EnemyView;
            var bossConfig = view.Config as BossConfig;
            var weaponConfigs = bossConfig.WeaponConfigs;

            foreach(var config in weaponConfigs)
            {
                var model = new WeaponModel(config, 0, 0);
                OnWeaponSelected(model, boss);
            }
        }
    }
}

