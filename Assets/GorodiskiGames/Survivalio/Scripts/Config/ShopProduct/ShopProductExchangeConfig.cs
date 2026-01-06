using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "Config/ShopProductExchangeConfig")]
    public sealed class ShopProductExchangeConfig : ShopProductConfig
    {
        public ResourceInfo Price;
    }
}
