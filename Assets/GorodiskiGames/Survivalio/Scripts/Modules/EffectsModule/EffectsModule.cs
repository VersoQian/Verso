using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Effect;
using Injection;
using UnityEngine;
using Game.Managers;
using Game.Enemy;
using Game.UI;

namespace Game.Modules
{
    public sealed class EffectsModule : Module<EffectsModuleView>
    {
        [Inject] private Timer _timer;
        [Inject] private GameManager _gameManager;
        [Inject] private LevelManager _levelManager;
        [Inject] private GameView _gameView;

        private bool _isPause;
        private readonly List<EffectView> _effects;
        private readonly Dictionary<ExplosionController, List<EnemyController>> _explosionsMap;

        public EffectsModule(EffectsModuleView view) : base(view)
        {
            _effects = new List<EffectView>();
            _explosionsMap = new Dictionary<ExplosionController, List<EnemyController>>();
        }

        public override void Initialize()
        {
            _gameManager.ON_SPAWN_EFFECT += OnSpawnEffect;
            _gameManager.ON_SPAWN_EXPLOSION += OnSpawnExplosion;
            _levelManager.ON_PAUSE += OnPause;
            _timer.TICK += OnTick;
        }

        public override void Dispose()
        {
            _gameManager.ON_SPAWN_EFFECT -= OnSpawnEffect;
            _gameManager.ON_SPAWN_EXPLOSION -= OnSpawnExplosion;
            _levelManager.ON_PAUSE -= OnPause;
            _timer.TICK -= OnTick;

            foreach (var pool in _view.EffectsMap.Values)
            {
                pool.ReleaseAllInstances();
            }
            _effects.Clear();
        }

        private void OnPause(bool pause)
        {
            _isPause = pause;
        }

        private void OnSpawnExplosion(Vector3 position, float damage)
        {
            var pool = _view.EffectsMap[EffectType.Explosion];
            var view = pool.Get<ExplosionView>();
            var radius = view.Radius;
            var searchEnemyRadius = radius * 1.3f;
            var explosion = new ExplosionController(view, position, damage, radius);

            var enemies = new List<EnemyController>();
            foreach (var enemy in _gameManager.Enemies)
            {
                var distance = Vector3.Distance(position, enemy.View.Position);
                if(distance > searchEnemyRadius)
                    continue;

                enemies.Add(enemy);
            }
            _explosionsMap.Add(explosion, enemies);

            _gameView.Camera.Shake();
        }

        private void OnSpawnEffect(EffectType type, Vector3 position, Vector3 direction)
        {
            var pool = _view.EffectsMap[type];
            var view = pool.Get<EffectView>();

            view.gameObject.SetActive(false);
            view.Init(type, position, direction);
            view.gameObject.SetActive(true);

            view.Play();

            view.ON_REMOVE += OnRemove;

            _effects.Add(view);
        }

        private void OnRemove(EffectView view)
        {
            view.ON_REMOVE -= OnRemove;

            _effects.Remove(view);

            var pool = _view.EffectsMap[view.Type];
            pool.Release(view);
        }

        private void RemoveExplosion(ExplosionController explosion)
        {
            var pool = _view.EffectsMap[EffectType.Explosion];
            pool.Release(explosion.View);
            _explosionsMap.Remove(explosion);
        }

        private void OnTick()
        {
            foreach (var view in _effects.ToList())
            {
                view.Proceed();
            }

            if (_isPause)
                return;

            foreach (var explosion in _explosionsMap.Keys.ToList())
            {
                var proceed = explosion.Proceed();
                if (!proceed)
                {
                    RemoveExplosion(explosion);
                    continue;
                }

                var explosionPosition = explosion.View.Position;
                var explosionRadius = explosion.Radius;
                var enemies = _explosionsMap[explosion];

                foreach (var enemy in enemies.ToList())
                {
                    var targetPosition = enemy.View.Position;
                    var distance = Vector3.Distance(explosionPosition, targetPosition);

                    var reachTarget = explosion.RadiusProgress > distance;
                    if (!reachTarget)
                        continue;

                    enemies.Remove(enemy);

                    var damage = Mathf.RoundToInt(explosion.DamageProgress);
                    var enemyAimPosition = enemy.View.AimPosition;
                    var direction = (explosionPosition - enemyAimPosition).normalized;

                    enemy.TryToDamage(damage, direction);
                }
            }
        }
    }
}

