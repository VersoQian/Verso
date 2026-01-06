using System.Collections.Generic;
using Game.Effect;
using Game.UI.Pool;
using UnityEngine;

namespace Game.Modules
{
    public sealed class EffectsModuleView : MonoBehaviour
    {
        [SerializeField] private ComponentPoolFactory _bloodPool;
        [SerializeField] private ComponentPoolFactory _explosionPool;

        public readonly Dictionary<EffectType, ComponentPoolFactory> EffectsMap;

        public EffectsModuleView()
        {
            EffectsMap = new Dictionary<EffectType, ComponentPoolFactory>();
        }

        public void Awake()
        {
            EffectsMap[EffectType.Blood] = _bloodPool;
            EffectsMap[EffectType.Explosion] = _explosionPool;
        }
    }
}

