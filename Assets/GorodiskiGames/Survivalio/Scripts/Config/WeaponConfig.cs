using System;
using UnityEngine;

namespace Game.Config
{
    public enum AttributeType
    {
        FireRate,
        Damage,
        Armor,
        Weight
    }

    [Serializable]
    public struct AttributeInfo
    {
        public AttributeType Type;
        public float Value; 
    }

    public enum BulletControllerType
    {
        Straight,
        RotateAround,
        Return,
        Parabola
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Config/WeaponConfig")]
    public sealed class WeaponConfig : EquipmentConfig
    {
        public float Range = 10f;
        public BulletControllerType BulletType;
        [Tooltip("Used to select the correct bullet prefab. See the spawn bullet logic in the WeaponsModule class.")]
        public int BulletIndex;
        [Tooltip("Default number of bullets.")]
        [Min(1)] public int BulletsCount = 1;
        public float BulletSpeed = 1f;
        [Min(0)] public float BulletLifeTime = 3f;
        [Tooltip("Delay when spawn bullets. Made for Straight BulletControllerType for visual purposes when BulletsCount is greater than 1.")]
        public float MultipleBulletsDelay;
        [Tooltip("Used for RotateAround BulletControllerType")]
        public float OrbitRadius = 3f;
    }
}

