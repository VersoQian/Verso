using System;
using Game.Modules;
using UnityEngine;

namespace Game.Config
{
    [Serializable]
    [CreateAssetMenu(menuName = "Config/EnemyConfig")]
    public class EnemyConfig : ScriptableObject
    {
        public float WalkSpeed = 5f;
        public int Health = 15;
        public int Damage = 15;
        [Tooltip("Color of damage notification text. Hardcoded Red if died. See the spawn logic in the UINotificationModule class.")]
        public UINotificationColorType UINotificationColor;
        [Tooltip("Resources that will be spawned after the death of an Enemy. See the spawn logic in the CollectiblesModule class.")]
        public ResourceInfo[] Reward;
    }
}

