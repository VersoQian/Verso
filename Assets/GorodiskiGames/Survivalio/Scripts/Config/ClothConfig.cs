using System;
using UnityEngine;

namespace Game.Config
{
    public enum ClothElementType
    {
        Top,        // 上衣 (74个)
        Bottom,     // 裤子 (67个)
        Headgear,   // 头部装备 (58个)
        Shoes,      // 鞋子 (45个)
        Gloves,     // 手套 (21个)
        Eyewear,    // 眼镜 (20个)
        Body,       // 身体部件 (18个)
        Bag,        // 背包 (17个)
        Face,       // 脸部变体 (10个)
        Hair        // 发型 (7个)
    }

    [Serializable]
    [CreateAssetMenu(menuName = "Config/ClothConfig")]
    public class ClothConfig : EquipmentConfig
    {
        public ClothElementType ClothType;
        public Mesh Mesh;
    }
}

