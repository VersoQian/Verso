using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Config
{
    public enum EntityType
    {
        Player,
        Weapon
    }

    [Serializable]
    public struct LogEntityVisibility
    {
        public EntityType Type;
        public bool ShowLog;
    }

    [CreateAssetMenu(menuName = "Config/GameConfig")]
    [ExecuteInEditMode]
    public sealed class GameConfig : ScriptableObject
    {
        public static GameConfig Load()
        {
            var result = Resources.Load<GameConfig>("GameConfig");
            result.Init();
            return result;
        }

        public GameConfig()
        {
            LogEntityMap = new Dictionary<EntityType, bool>();
            ShopProductsIAPMap = new Dictionary<string, ShopProductIAPConfig>();
            WeaponMap = new Dictionary<int, WeaponConfig>();
            AbilityMap = new Dictionary<AbilityType, AbilityConfig>();
            ClothMap = new Dictionary<int, ClothConfig>();
            ResourcesMap = new Dictionary<ResourceItemType, ResourceConfig>();
        }

        private void Init()
        {
            foreach (EntityType type in Enum.GetValues(typeof(EntityType)))
            {
                LogEntityMap[type] = false;
            }

            foreach (var item in _logEntity)
            {
                LogEntityMap[item.Type] = item.ShowLog;
            }

            for (int i = 0; i < _weaponConfigs.Length; i++)
            {
                var config = _weaponConfigs[i];
                config.Index = i;

                if(WeaponMap.ContainsValue(config))
                    Log.Warning($"Duplicate WeaponConfig found at index {i}: {config.name}");

                WeaponMap[i] = config;
            }

            for (int i = 0; i < _abilityConfigs.Length; i++)
            {
                var config = _abilityConfigs[i];
                var type = config.Type;

                if(AbilityMap.ContainsKey(type))
                    Log.Warning($"Duplicate AbilityConfig found at index {i}: {config.name}");

                AbilityMap[type] = config;
            }

            for (int i = 0; i < _clothConfigs.Length; i++)
            {
                var config = _clothConfigs[i];
                config.Index = i;

                if(ClothMap.ContainsValue(config))
                    Log.Warning($"Duplicate ClothConfig found at index {i}: {config.name}");

                ClothMap[i] = config;
            }

            for (int i = 0; i < _resourceConfigs.Length; i++)
            {
                var config = _resourceConfigs[i];
                var type = config.ResourceType;

                if (ResourcesMap.ContainsValue(config))
                    Log.Warning($"Duplicate ResourceConfig found at index {i}: {config.name}");

                ResourcesMap[type] = config;
            }

            foreach (var product in _shopProductIAPConfigs)
            {
                ShopProductsIAPMap[product.ID] = product;
            }
        }

        public readonly Dictionary<EntityType, bool> LogEntityMap;
        public readonly Dictionary<string, ShopProductIAPConfig> ShopProductsIAPMap;
        public readonly Dictionary<ResourceItemType, ResourceConfig> ResourcesMap; //type, config
        public readonly Dictionary<int, WeaponConfig> WeaponMap; //index, config
        public readonly Dictionary<AbilityType, AbilityConfig> AbilityMap; //type, config
        public readonly Dictionary<int, ClothConfig> ClothMap; //index, config

        [Header("Defaults")]
        [Min(0)] public int DefaultLevel;
        [Min(0)] public int DefaultEnergy;
        [Min(0)] public int DefaultGems;
        [Min(0)] public int DefaultCash;
        public MenuHudType DefaultMenuHud = MenuHudType.Battle;
        [Range(0f, 1f)] public float MusicVolumeDefault = 0.5f;
        [Range(0f, 1f)] public float SFXVolumeDefault = 0.5f;

        [Header("Splash Screen")]
        public float SplashScreenDurationMobile = 1.5f;
        public float SplashScreenDurationEditor = 0.1f;

        [Header("Logs")]
        [SerializeField] private LogEntityVisibility[] _logEntity;

        [Header("Energy")]
        [Min(1)] public int EnergyMax = 60;
        [Tooltip("The time (in seconds) it takes for the Energy to be restored.")]
        [Min(1)] public int EnergyRestoreInterval = 10;

        [Header("Player")]
        [Tooltip("Set to true to add all Cloth (from the ClothConfigs list) and Weapons (from the WeaponConfigs list) at the very first start of the game. Made for testing purposes.")]
        public bool AddAllEquipment;
        public PlayerConfig PlayerConfig;

        [Header("Levels")]
        [Tooltip("List of levels in the game. Any new level should be added to this list.")]
        public LevelConfig[] LevelConfigs;

        [Header("Resources")]
        [Tooltip("List of Resources in the game (eg Сash, Gems).")]
        [SerializeField] private ResourceConfig[] _resourceConfigs;

        [Header("Weapons")]
        [Tooltip("List of Weapons in the game. Any new Weapon should be added to this list. The very first config will be set as the default Weapon at the very first start of the game.")]
        [SerializeField] private WeaponConfig[] _weaponConfigs;

        [Header("Abilities")]
        [Tooltip("List of Abilities in the game. Any new Ability should be added to this list.")]
        [SerializeField] private AbilityConfig[] _abilityConfigs;

        [Header("Cloth")]
        [Tooltip("List of Cloth in the game. Any new Cloth should be added to this list. Must contains all types of Cloth from ClothElementType (see ClothConfig class) in order to select the default Cloth at the very first start of the game.")]
        [SerializeField] private ClothConfig[] _clothConfigs;

        [Header("Menu")]
        public MenuHudConfig[] MenuHudConfigs;

        [Header("Shop")]
        [Tooltip("List of in-app purchases (IAP) for real money.")]
        [SerializeField] private ShopProductIAPConfig[] _shopProductIAPConfigs;
    }
}
