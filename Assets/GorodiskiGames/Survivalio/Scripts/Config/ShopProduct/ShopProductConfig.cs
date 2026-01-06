using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "Config/ShopProductConfig")]
    public class ShopProductConfig : ScriptableObject
    {
        public string ID;
        public ShopProductType ProductType;
        public ResourceInfo Reward;
    }

    public enum ShopProductType
    {
        ResourcesForRealMoney,
        ResourcesExchange
    }
}
