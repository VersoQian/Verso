using System;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "Config/BossConfig")]
    public sealed class BossConfig : EnemyConfig
    {
        [Tooltip("List of weapons for Boss.")]
        public WeaponConfig[] WeaponConfigs;
    }
}


