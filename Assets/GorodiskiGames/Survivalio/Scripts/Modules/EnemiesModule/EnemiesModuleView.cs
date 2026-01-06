using Game.Enemy;
using Game.UI.Pool;
using UnityEngine;

namespace Game.Modules
{
    public sealed class EnemiesModuleView : MonoBehaviour
    {
        public ComponentPoolFactory[] Enemies;
        public ComponentPoolFactory BossPool;
    }
}

