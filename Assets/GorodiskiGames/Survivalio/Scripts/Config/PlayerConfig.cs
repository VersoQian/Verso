using System;
using Game.Player;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    public struct UnitAttributeInfo
    {
        public UnitAttributeType Type;
        public float Value;
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Config/PlayerConfig")]
    public sealed class PlayerConfig : ScriptableObject
    {
        public Sprite Icon;
        public string Label;
        [Tooltip("The nominal Health of the Player to which the Armor attribute (AttributeType.Armor) from each of Cloth will be added.")]
        public int Health;
        public float WalkSpeed = 5f;
        public float RotateSpeed = 20f;
        [Tooltip("Set to true if you have assigned a Mesh to the all ClothConfigs in the GameConfig.")]
        public bool HasAllClothMeshes;
        [Tooltip("Assign your charactes Mesh if HasAllClothMeshes is false.")]
        public Mesh FullSkinnedMesh;
        [Tooltip("Attributes that will be affected by abilities selected during gameplay.")]
        public UnitAttributeInfo[] AttributeInfos;
    }
}
