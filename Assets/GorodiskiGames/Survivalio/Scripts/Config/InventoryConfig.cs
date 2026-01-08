using UnityEngine;
using System;

namespace Game.Config
{
    public enum InventoryCategory
    {
        Cloth,
        Weapon,
        Resource
    }

    [Serializable]
    public struct InventoryInfo
    {
        public InventoryConfig Inventory;
        [Tooltip("The value is used only for the Resource (Credit, PassCard) InventoryCategory. For the Cloth and Weapon InventoryCategory hardcoded to 1.")]
        public int Amount;
    }

    [Serializable]
    public abstract class InventoryConfig : ScriptableObject
    {
        public string Label;
        public Sprite Icon;
        public InventoryCategory Category;
    }
}

