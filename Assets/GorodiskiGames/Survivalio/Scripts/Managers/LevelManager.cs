using Game.Core;
using Injection;
using System;
using Core;
using UnityEngine;
using Game.Config;
using Game.Utilities;
using System.Collections.Generic;
using Game.Weapon;
using Game.Unit;
using Game.Enemy;
using Game.Ability;
using Game.Player;

namespace Game.Managers
{
    public sealed class LevelModel : Observable
    {
        public float Elapsed;
        public float Duration;
        public float DurationMax;
        public float SpawnBossTime;
        public int Level;
        public int EnemiesKilled;
        public int Hearts;
        public string Label;
        public GameObject Prefab;
        public int SkillMilestoneIndex;
        public int ResourcesAmountOnStart;
        public float EnemiesOnLevelStart;
        public float EnemiesOnLevelEnd;
        public InventoryInfo[] InventoryInfos;

        private readonly SkillsInfo[] _skillsInfos;
        public readonly Dictionary<ResourceItemType, int> CollectedResourcesMap;
        public readonly Dictionary<ResourceInfo, float> SpawnResourcesMap;

        public LevelModel(LevelConfig config, int level, float durationMax)
        {
            Label = config.Label;
            Prefab = config.Prefab;
            ResourcesAmountOnStart = config.ResourcesAmountOnStart;
            _skillsInfos = config.SkillsInfos;
            InventoryInfos = config.InventoryInfos;
            Duration = config.Duration;
            EnemiesOnLevelStart = config.EnemiesOnLevelStart;
            EnemiesOnLevelEnd = config.EnemiesOnLevelEnd;
            Level = level;
            EnemiesKilled = 0;
            Hearts = 0;
            SkillMilestoneIndex = 0;
            DurationMax = durationMax;
            SpawnBossTime = Duration * config.BossSpawnDelay / 100f;

            SpawnResourcesMap = new Dictionary<ResourceInfo, float>();
            foreach (var spawnInfo in config.ResourceInfos)
            {
                var delay = spawnInfo.Delay;
                var spawnTime = Duration * delay / 100f;
                SpawnResourcesMap[spawnInfo.Info] = spawnTime;
            }

            CollectedResourcesMap = new Dictionary<ResourceItemType, int>();
            foreach (ResourceItemType type in Enum.GetValues(typeof(ResourceItemType)))
            {
                CollectedResourcesMap[type] = 0;
            }
        }

        public int ResourcesAmount => _skillsInfos[SkillMilestoneIndex].ResourcesAmount;
        public int MilestonesCount => _skillsInfos.Length;
        public WeaponConfig[] WeaponConfigs => _skillsInfos[SkillMilestoneIndex].WeaponConfigs;
        public AbilityConfig[] AbilityConfigs => _skillsInfos[SkillMilestoneIndex].AbilityConfigs;
        public bool IsNewRecord { get; internal set; }
        public bool IsReachSpawnBossTime => Elapsed >= SpawnBossTime;

        public bool ReachMilestone => GemsGreen >= ResourcesAmount;

        public int GemsGreen
        {
            get { return CollectedResourcesMap[ResourceItemType.GemsGreen]; }
            set { CollectedResourcesMap[ResourceItemType.GemsGreen] = value; }
        }

        public int Cash
        {
            get { return CollectedResourcesMap[ResourceItemType.Cash]; }
        }
    }

    public sealed class LevelManager
    {
        public event Action<bool> ON_PAUSE;
        public event Action ON_REACH_SKILL_MILESTONE;
        public event Action<bool> ON_LEVEL_END;
        public event Action ON_REACH_SPAWN_BOSS_TIME;
        public event Action<EnemyController> ON_BOSS_SPAWNED;
        public event Action<UnitAttributeType> ON_ATTRIBUTE_UPDATED;

        [Inject] private Timer _timer;

        private bool _gameEnd;
        private bool _pause;

        private readonly LevelModel _model;
        private readonly TimerDelayer _timerDelayer;
        private readonly Dictionary<WeaponController, UnitController> _weapons;
        private readonly List<AbilityController> _abilities;

        public bool GameEnd => _gameEnd;
        public LevelModel Model => _model;
        public Dictionary<WeaponController, UnitController> WeaponsMap => _weapons;
        public List<AbilityController> Abilities => _abilities;

        public bool IsPause => _pause;

        public LevelManager(LevelConfig config, int level, float durationMax)
        {
            _model = new LevelModel(config, level, durationMax);
            _weapons = new Dictionary<WeaponController, UnitController>();
            _abilities = new List<AbilityController>();
            _timerDelayer = new TimerDelayer();
        }

        public void Initialize()
        {
            _gameEnd = false;

            _timer.TICK += OnTick;
            _timer.ONE_SECOND_TICK += OnSecondTick;
        }

        public void Dispose()
        {
            _timer.TICK -= OnTick;
            _timer.ONE_SECOND_TICK -= OnSecondTick;
        }

        private void OnTick()
        {
            _timerDelayer.Tick();
        }

        public void Pause()
        {
	        _pause = true;
            ON_PAUSE?.Invoke(_pause);
        }

		public void Unpause()
		{
	        _pause = false;
            ON_PAUSE?.Invoke(_pause);
        }

        private void OnSecondTick()
        {
	        if (_gameEnd || _pause)
                return;

            _model.Elapsed++;
            _model.SetChanged();

            if (!_model.IsReachSpawnBossTime)
                return;

            _timer.ONE_SECOND_TICK -= OnSecondTick;

            ON_REACH_SPAWN_BOSS_TIME?.Invoke();
        }

        public void FireLevelEnd(bool isWin)
        {
            _gameEnd = true;
            ON_LEVEL_END?.Invoke(isWin);
        }

        public void FireBossSpawned(EnemyController boss)
        {
            ON_BOSS_SPAWNED?.Invoke(boss);
        }

        public void FireAttributeUpdated(UnitAttributeType attributeType)
        {
            ON_ATTRIBUTE_UPDATED?.Invoke(attributeType);
        }

        public void AddResource(ResourceItemType resourceType, int value)
        {
            _model.CollectedResourcesMap[resourceType] += value;
            _model.SetChanged();

            CheckIfReachedSkillMilestone();
        }

        private void CheckIfReachedSkillMilestone()
        {
            if (!_model.ReachMilestone || _model.IsReachSpawnBossTime)
                return;

            ON_REACH_SKILL_MILESTONE?.Invoke();

            var milestonesCount = _model.MilestonesCount;
            var skillMilestoneIndex = _model.SkillMilestoneIndex;
            skillMilestoneIndex++;

            if (skillMilestoneIndex >= milestonesCount)
                skillMilestoneIndex = 0;

            _model.SkillMilestoneIndex = skillMilestoneIndex;
            _model.GemsGreen = 0;
        }
    }
}

