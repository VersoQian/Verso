using System;
using UnityEngine;

namespace Game.Config
{
    public class EquipmentConfig : InventoryConfig
    {
        [HideInInspector] public int Index;
        [Tooltip("Mandatory attributes for Cloth: Armor, Weight. For Weapon: FireRate, Damage.")]
        public EquipmentLevelConfig[] Levels;
    }

    [Serializable]
    public sealed class EquipmentLevelConfig
    {
        public int Price;
        public AttributeInfo[] Attributes;
    }
}

