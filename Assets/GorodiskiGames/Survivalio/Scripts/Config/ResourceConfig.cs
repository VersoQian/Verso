using System;
using UnityEngine;

namespace Game.Config
{
    public enum ResourceItemType
    {
        Credit,
        PassCard,
        Energy,
        GemsGreen
    }

    [Serializable]
    public struct ResourceInfo
    {
        public ResourceItemType ResourceType;
        public int Amount;
    }

    [Serializable]
    public struct ResourceSpawnInfo
    {
        [Tooltip("Delay (in percent) from the start of the level until spawn resources.")]
        [Range(1, 100)] public int Delay;
        public ResourceInfo Info;
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Config/ResourceConfig")]
    public class ResourceConfig : InventoryConfig
    {
        public ResourceItemType ResourceType;
    }
}

