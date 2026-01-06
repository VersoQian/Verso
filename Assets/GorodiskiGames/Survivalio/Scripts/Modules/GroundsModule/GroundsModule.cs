using System.Collections.Generic;
using System.Linq;
using Game.Core;
using Game.Level;
using Game.Managers;
using Injection;
using UnityEngine;
using Utilities;

namespace Game.Modules
{
    public sealed class GroundsModule : Module
    {
        private const float _groundSize = 100f;
        private const float _addTileDistanceFactor = 0.7f;
        private const float _removeTileDistanceFactor = 3f;

        [Inject] private GameManager _gameManager;
        [Inject] private Timer _timer;
        [Inject] private LevelView _levelView;

        private Vector3 _cachedPosition;

        private readonly Dictionary<Vector3, GroundView> _groundTitlesMap;

        public GroundsModule()
        {
            _groundTitlesMap = new Dictionary<Vector3, GroundView>();
        }

        public override void Initialize()
        {
            Rearrange(_gameManager.Player.View.Position);

            _timer.FIXED_TICK += OnFixedTick;
            _timer.ONE_SECOND_TICK += OnSecondTick;
        }

        public override void Dispose()
        {
            _timer.FIXED_TICK -= OnFixedTick;
            _timer.ONE_SECOND_TICK -= OnSecondTick;
        }

        private void OnFixedTick()
        {
            foreach (var tilePosition in _groundTitlesMap.Keys.ToList())
            {
                if (tilePosition == _cachedPosition)
                    continue;

                var distance = Vector3.Distance(_gameManager.Player.View.Position, tilePosition);
                if (distance > _groundSize * _addTileDistanceFactor)
                    continue;

                Rearrange(tilePosition);
            }
        }

        private void OnSecondTick()
        {
            foreach (var tilePosition in _groundTitlesMap.Keys.ToList())
            {
                var distance = Vector3.Distance(_gameManager.Player.View.Position, tilePosition);
                if (distance < _groundSize * _removeTileDistanceFactor)
                    continue;

                var tile = _groundTitlesMap[tilePosition];
                var pool = _levelView.GroundPools[tile.Index];
                pool.Release(tile);

                _groundTitlesMap.Remove(tilePosition);
            }
        }

        private void Rearrange(Vector3 position)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    var positionRaw = position + new Vector3(x * _groundSize, 0f, z * _groundSize);
                    var positionResult = MathUtil.RoundToTwoDecimals(positionRaw);
                    
                    if (_groundTitlesMap.ContainsKey(positionResult))
                        continue;

                    var index = Random.Range(0, _levelView.GroundPools.Length);
                    var pool = _levelView.GroundPools[index];

                    var ground = pool.Get<GroundView>();

                    ground.Index = index;
                    ground.Position = positionResult;

                    _groundTitlesMap.Add(positionResult, ground);
                }
            }
            _cachedPosition = position;
        }
    }
}

