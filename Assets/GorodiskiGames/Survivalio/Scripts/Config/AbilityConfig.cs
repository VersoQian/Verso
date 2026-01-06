using System;
using UnityEngine;

namespace Game.Config
{
    public enum AbilityType
    {
        Magnet,
        EnergyDrink
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Config/AbilityConfig")]
    public sealed class AbilityConfig : ScriptableObject
    {
        public AbilityType Type;
        public string Label;
        public Sprite Icon;
        [Tooltip("Default count of ability.")]
        [Min(1)] public int Count = 1;
        [Tooltip("Attribute that will be added to the player's corresponding attribute.")]
        public UnitAttributeInfo AttributeInfo;
    }
}

