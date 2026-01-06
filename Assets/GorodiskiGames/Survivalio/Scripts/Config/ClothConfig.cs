using System;
using UnityEngine;

namespace Game.Config
{
    public enum ClothElementType
    {
        Headgear,   // 头部装备
        Top,        // 上衣
        Gloves,     // 手套
        Bottom,     // 裤子
        Shoes       // 鞋子
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Config/ClothConfig")]
    public class ClothConfig : EquipmentConfig
    {
        public ClothElementType ClothType;
        public Mesh Mesh;
    }
}

