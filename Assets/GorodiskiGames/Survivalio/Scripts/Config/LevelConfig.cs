using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    public struct SkillsInfo
    {
        [Tooltip("The amount of resources (GemsGreen by default) must be collected.")]
        [Min(1)] public int ResourcesAmount;
        [Tooltip("Weapons that will be offered to choose from.")]
        public WeaponConfig[] WeaponConfigs;
        [Tooltip("Abilities that will be offered to choose from.")]
        public AbilityConfig[] AbilityConfigs;
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Config/LevelConfig")]
    public sealed class LevelConfig : ScriptableObject
    {
        public string Label;
        [Tooltip("Used as the actual level where all gameplay occurs.")]
        public GameObject Prefab;
        [Tooltip("Used as a level preview image in the BattleHud (2D Sprite).")]
        public Sprite PreviewImage;
        [Tooltip("(Deprecated) Used as a 3D level preview in the BattleHud.")]
        public GameObject Preview;
        [Tooltip("The amount of resources (GemsGreen by default) will be spawned at the start of the level.")]
        public int ResourcesAmountOnStart = 50;
        [Tooltip("Level duration in seconds.")]
        public float Duration = 60;
        [Tooltip("The amount of enemies (per second) will be spawned at the start of the level. See the spawn logic in the EnemiesModule class.")]
        [Min(1)] public int EnemiesOnLevelStart = 5;
        [Tooltip("The amount of enemies (per second) will be spawned at the end of the level.")]
        [Min(1)] public int EnemiesOnLevelEnd = 10;
        [Tooltip("Delay (in percent) from the start of the level until spawn the Boss. See the spawn logic in the EnemiesModule class.")]
        [Range(1, 100)] public int BossSpawnDelay = 99;
        [Space]
        [Tooltip("Weapons that will be sugested to select during the level.")]
        public SkillsInfo[] SkillsInfos;
        [Tooltip("Resources that will be spawned during the level.")]
        public ResourceSpawnInfo[] ResourceInfos;
        [Tooltip("Inventory that will be given upon victory. (Cloth, Weapon, Resource)")]
        public InventoryInfo[] InventoryInfos;
    }
}

        