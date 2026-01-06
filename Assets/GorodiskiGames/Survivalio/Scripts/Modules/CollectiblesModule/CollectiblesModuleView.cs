using System.Collections.Generic;
using Game.Config;
using Game.UI.Pool;
using UnityEngine;

namespace Game.Modules
{
    public sealed class CollectiblesModuleView : MonoBehaviour
    {
        [SerializeField] private ComponentPoolFactory _cashPool;
        [SerializeField] private ComponentPoolFactory _gemsGreenPool;
        [SerializeField] private ComponentPoolFactory _gemsPinkPool;

        public readonly Dictionary<ResourceItemType, ComponentPoolFactory> ResourcesMap;

        public CollectiblesModuleView()
        {
            ResourcesMap = new Dictionary<ResourceItemType, ComponentPoolFactory>();
        }

        private void Awake()
        {
            ResourcesMap[ResourceItemType.Cash] = _cashPool;
            ResourcesMap[ResourceItemType.GemsGreen] = _gemsGreenPool;
            ResourcesMap[ResourceItemType.GemsPink] = _gemsPinkPool;
        }
    }
}

