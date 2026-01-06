using System;
using UnityEngine;
using UnityEngine.Purchasing;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "Config/ShopProductIAPConfig")]
    public sealed class ShopProductIAPConfig : ShopProductConfig
    {
        public ProductType Type;
    }
}