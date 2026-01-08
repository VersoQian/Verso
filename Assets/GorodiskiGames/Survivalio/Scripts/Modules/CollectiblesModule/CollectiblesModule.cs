using System.Collections.Generic;
using System.Linq;
using Game.Collectible;
using Game.Config;
using Game.Core;
using Game.Enemy;
using Game.Managers;
using Injection;
using UnityEngine;

namespace Game.Modules
{
    public enum CollectibleStateType
    {
        OnStart,
        Intro,
        Idle
    }

    public sealed class CollectiblesModule : Module<CollectiblesModuleView>
    {
        private const float _rangeMin = 3.5f;
        private const float _rangeMax = 6f;
        private const float _distance = 8f;

        [Inject] private GameManager _gameManager;
        [Inject] private Context _context;
        [Inject] private LevelManager _levelManager;
        [Inject] private Timer _timer;

        private readonly List<CollectibleController> _collectibles;

        public CollectiblesModule(CollectiblesModuleView view) : base(view)
        {
            _collectibles = new List<CollectibleController>();
        }

        public override void Initialize()
        {
            var resourcesAmountOnStart = _levelManager.Model.ResourcesAmountOnStart;
            SpawnCollectiblesPool(ResourceItemType.GemsGreen, resourcesAmountOnStart, _rangeMin, _rangeMax, _gameManager.Player.View.Position, CollectibleStateType.OnStart);

            _gameManager.ON_ENEMY_DIED += OnEnemyDied;
            _levelManager.ON_LEVEL_END += OnLevelEnd;
            _timer.ONE_SECOND_TICK += OnSecondTick;
        }

        public override void Dispose()
        {
            _gameManager.ON_ENEMY_DIED -= OnEnemyDied;
            _levelManager.ON_LEVEL_END -= OnLevelEnd;
            _timer.ONE_SECOND_TICK -= OnSecondTick;

            foreach (var collectible in _collectibles)
            {
                collectible.ON_COLLECTED -= OnCollected;
                collectible.Dispose();
            }
            _collectibles.Clear();

            foreach (var pool in _view.ResourcesMap.Values)
            {
                if (pool != null)
                    pool.ReleaseAllInstances();
            }
        }

        private void OnSecondTick()
        {
            var spawnResourcesMap = _levelManager.Model.SpawnResourcesMap;
            var elapsed = _levelManager.Model.Elapsed;
            var position = _gameManager.Player.View.Position + _gameManager.Player.View.RotateNode.forward * _distance;

            foreach (var info in spawnResourcesMap.Keys.ToList())
            {
                var spawnTime = spawnResourcesMap[info];
                if(elapsed < spawnTime)
                    continue;

                spawnResourcesMap.Remove(info);

                var resourceType = info.ResourceType;
                var amount = info.Amount;

                SpawnCollectiblesPool(resourceType, amount, 0f, _rangeMax, position, CollectibleStateType.Idle);
            }
        }

        private void SpawnCollectiblesPool(ResourceItemType resourceType, int resourceAmount, float rangeMin, float rangeMax, Vector3 startPosition, CollectibleStateType state)
        {
            for (int i = 0; i < resourceAmount; i++)
            {
                var direction = Random.insideUnitCircle.normalized;
                var directionResult = new Vector3(direction.x, 0f, direction.y);
                var rangeResult = Random.Range(rangeMin, rangeMax);
                var endPosition = startPosition + directionResult * rangeResult;
                CreateCollectible(resourceType, startPosition, endPosition, state);
            }
        }

        private void OnLevelEnd(bool isWin)
        {
            _levelManager.ON_LEVEL_END -= OnLevelEnd;
            _timer.ONE_SECOND_TICK -= OnSecondTick;

            foreach (var collectible in _collectibles)
            {
                collectible.ON_COLLECTED -= OnCollected;
            }
        }

        private void OnEnemyDied(EnemyController enemy)
        {
            foreach (var reward in enemy.Model.Reward)
            {
                var resourceType = reward.ResourceType;
                var resourceAmount = reward.Amount;
                var startPosition = enemy.View.Position;

                SpawnCollectiblesPool(resourceType, resourceAmount, _rangeMin, _rangeMax, startPosition, CollectibleStateType.Intro);
            }
        }

        private void CreateCollectible(ResourceItemType resourceType, Vector3 startPosition, Vector3 endPosition, CollectibleStateType state)
        {
            if (!_view.ResourcesMap.ContainsKey(resourceType))
            {
                Debug.LogError($"[CollectiblesModule] ResourcesMap不包含资源类型: {resourceType}");
                return;
            }

            var pool = _view.ResourcesMap[resourceType];
            if (pool == null)
            {
                Debug.LogError($"[CollectiblesModule] Pool为null,资源类型: {resourceType}");
                return;
            }

            var view = pool.Get<CollectibleView>();

            if(state != CollectibleStateType.Intro)
                startPosition = endPosition;

            view.Initialize(startPosition);

            var collectible = new CollectibleController(view, resourceType, _context);

            _collectibles.Add(collectible);

            collectible.ON_COLLECTED += OnCollected;

            if(state == CollectibleStateType.OnStart)
                collectible.OnStart();
            else if(state == CollectibleStateType.Intro)
                collectible.Intro(endPosition);
            else if (state == CollectibleStateType.Idle)
                collectible.Idle();
        }

        private void OnCollected(CollectibleController collectible)
        {
            collectible.ON_COLLECTED -= OnCollected;
            collectible.Dispose();

            _collectibles.Remove(collectible);

            var resourceType = collectible.ResourceType;
            _view.ResourcesMap[resourceType].Release(collectible.View);

            _levelManager.AddResource(resourceType, 1);
        }
    }
}

