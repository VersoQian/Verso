using System.Collections.Generic;
using Game.Config;
using Game.UI.Pool;
using UnityEngine;

namespace Game.Modules
{
    public sealed class CollectiblesModuleView : MonoBehaviour
    {
        [SerializeField] private ComponentPoolFactory _cashPool;        // 内部使用Credit枚举
        [SerializeField] private ComponentPoolFactory _gemsGreenPool;
        [SerializeField] private ComponentPoolFactory _gemsPinkPool;    // 内部使用PassCard枚举

        public readonly Dictionary<ResourceItemType, ComponentPoolFactory> ResourcesMap;

        public CollectiblesModuleView()
        {
            ResourcesMap = new Dictionary<ResourceItemType, ComponentPoolFactory>();
        }

        private void Awake()
        {
            ResourcesMap[ResourceItemType.Credit] = _cashPool;          // 字段名保持_cashPool,但映射到Credit
            ResourcesMap[ResourceItemType.GemsGreen] = _gemsGreenPool;
            ResourcesMap[ResourceItemType.PassCard] = _gemsPinkPool;    // 字段名保持_gemsPinkPool,但映射到PassCard

            // 调试日志:检查pool是否正确分配
            Debug.Log($"[CollectiblesModuleView] Credit Pool (_cashPool): {(_cashPool != null ? "已分配" : "NULL")}");
            Debug.Log($"[CollectiblesModuleView] GemsGreen Pool: {(_gemsGreenPool != null ? "已分配" : "NULL")}");
            Debug.Log($"[CollectiblesModuleView] PassCard Pool (_gemsPinkPool): {(_gemsPinkPool != null ? "已分配" : "NULL")}");
        }
    }
}

