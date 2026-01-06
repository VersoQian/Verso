using Game.Core;
using Game.Enemy;
using Game.Managers;
using Game.UI.Hud;
using Injection;
using UnityEngine;

namespace Game.Modules
{
    public enum TeamIDType
    {
        Player,
        Enemy
    }

    public sealed class EnemiesModule : Module<EnemiesModuleView>
    {
        private const float _enemyShiftSpawnPosition = 40f;
        private const float _bossDistance = 20f;

        [Inject] private Timer _timer;
        [Inject] private Context _context;
        [Inject] private LevelManager _levelManager;
        [Inject] private GameManager _gameManager;
        [Inject] private HudManager _hudManager;

        private bool _isPause;

        public EnemiesModule(EnemiesModuleView view): base(view)
        {

        }

        public override void Initialize()
        {
            _levelManager.ON_PAUSE += OnPause;
            _levelManager.ON_LEVEL_END += OnLevelEnd;
            _levelManager.ON_REACH_SPAWN_BOSS_TIME += OnReachSpawnBossTime;
            _levelManager.ON_BOSS_SPAWNED += OnBossSpawned;

            _gameManager.ON_ENEMY_DIED += OnEnemyDied;

            _timer.ONE_SECOND_TICK += OnSecondTickSpawnEnemies;
            _timer.ONE_SECOND_TICK += OnSecondTickRefillCachedEnemies;
        }

        public override void Dispose()
        {
            _levelManager.ON_PAUSE -= OnPause;
            _levelManager.ON_LEVEL_END -= OnLevelEnd;
            _levelManager.ON_REACH_SPAWN_BOSS_TIME -= OnReachSpawnBossTime;
            _levelManager.ON_BOSS_SPAWNED -= OnBossSpawned;

            _gameManager.ON_ENEMY_DIED -= OnEnemyDied;

            _timer.ONE_SECOND_TICK -= OnSecondTickSpawnEnemies;
            _timer.ONE_SECOND_TICK -= OnSecondTickRefillCachedEnemies;

            foreach (var enemy in _gameManager.Enemies)
            {
                enemy.Dispose();
            }
            _gameManager.Enemies.Clear();

            foreach (var pool in _view.Enemies)
            {
                pool.ReleaseAllInstances();
            }

            _hudManager.HideAdditional<BossIncomingHudMediator>();
        }

        public void OnSecondTickRefillCachedEnemies()
        {
            if (_isPause)
                return;

            _gameManager.RefillCachedEnemies();
        }

        public void OnSecondTickSpawnEnemies()
        {
            if (_isPause)
                return;

            var enemiesOnLevelStart = _levelManager.Model.EnemiesOnLevelStart;
            var enemiesOnLevelEnd = _levelManager.Model.EnemiesOnLevelEnd;
            var elapsed = _levelManager.Model.Elapsed;
            var duration = _levelManager.Model.Duration;
            var t = Mathf.Clamp01(elapsed / duration);
            var enemiesResult = (int)Mathf.Lerp(enemiesOnLevelStart, enemiesOnLevelEnd, t);

            for (int i = 0; i < enemiesResult; i++)
            {
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            var index = Random.Range(0, _view.Enemies.Length);
            var pool = _view.Enemies[index];
            var view = pool.Get<EnemyView>();
            view.Position = GetEnemyPosition();
            var isBoss = false;
            var enemy = new EnemyController(view, _context, isBoss)
            {
                Index = index
            };

            _gameManager.AddEnemy(enemy);
            enemy.FollowPlayer();
        }

        private void OnPause(bool pause)
        {
            _isPause = pause;
        }

        private void OnEnemyDied(EnemyController enemy)
        {
            ReleaseEnemy(enemy);

            if(!enemy.IsBoss)
                return;

            var isWin = true;
            _levelManager.FireLevelEnd(isWin);
        }

        private void ReleaseEnemy(EnemyController enemy)
        {
            _gameManager.RemoveEnemy(enemy);
            enemy.Dispose();

            var index = enemy.Index;
            var pool = _view.Enemies[index];

            if(enemy.IsBoss)
                pool = _view.BossPool;

            pool.Release(enemy.View);
        }

        private void OnLevelEnd(bool isWin)
        {
            _levelManager.ON_LEVEL_END -= OnLevelEnd;

            _timer.ONE_SECOND_TICK -= OnSecondTickSpawnEnemies;
            _timer.ONE_SECOND_TICK -= OnSecondTickRefillCachedEnemies;

            foreach (var enemy in _gameManager.Enemies)
            {
                if(isWin)
                    enemy.Die();
                else
                    enemy.Idle();
            }

            _hudManager.HideAdditional<BossIncomingHudMediator>();
        }

        private Vector3 GetEnemyPosition()
        {
            var randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            var target  = _gameManager.Player.View.transform;

            var result = new Vector3(
                target.position.x + Mathf.Cos(randomAngle) * _enemyShiftSpawnPosition,
                target.position.y,
                target.position.z + Mathf.Sin(randomAngle) * _enemyShiftSpawnPosition
            );

            return result;
        }

        private void OnReachSpawnBossTime()
        {
            _levelManager.ON_REACH_SPAWN_BOSS_TIME -= OnReachSpawnBossTime;
            _timer.ONE_SECOND_TICK -= OnSecondTickSpawnEnemies;

            var view = _view.BossPool.Get<EnemyView>();
            view.Position = GetBossPosition();
            var boss = new EnemyController(view, _context, true);
            _gameManager.Boss = boss;

            _gameManager.AddEnemy(boss);
            boss.FollowPlayer();

            _levelManager.FireBossSpawned(boss);
        }

        private void OnBossSpawned(EnemyController enemy)
        {
            _levelManager.ON_BOSS_SPAWNED -= OnBossSpawned;

            _hudManager.ShowAdditional<BossIncomingHudMediator>();
        }

        private Vector3 GetBossPosition()
        {
            var playerPosition = _gameManager.Player.View.Position;
            var result = playerPosition + Vector3.forward * _bossDistance;
            return result;
        }
    }
}

