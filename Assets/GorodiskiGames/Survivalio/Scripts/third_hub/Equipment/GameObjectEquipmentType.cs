namespace Game.ThirdHub.Equipment
{
    /// <summary>
    /// 基于GameObject激活的装备部位类型
    /// 适用于 Character_114 等通过 GameObject 显示/隐藏来切换装备的角色模型
    /// </summary>
    public enum GameObjectEquipmentType
    {
        None = 0,

        /// <summary>背包</summary>
        Bag = 1,

        /// <summary>下装/裤子</summary>
        Bottom = 2,

        /// <summary>眼镜</summary>
        Eyewear = 3,

        /// <summary>手套</summary>
        Glove = 4,

        /// <summary>头发</summary>
        Hair = 5,

        /// <summary>头部装备</summary>
        Headgear = 6,

        /// <summary>鞋子</summary>
        Shoes = 7,

        /// <summary>上衣</summary>
        Top = 8,

        /// <summary>身体上半部分（如果使用分体模型）</summary>
        BodyTop = 9,

        /// <summary>身体下半部分（如果使用分体模型）</summary>
        BodyBottom = 10
    }
}
