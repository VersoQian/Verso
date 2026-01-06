using Game.Core;
using Injection;
using UnityEngine;

namespace Game.Enemy.States
{
    public abstract class EnemyState : State
    {
        [Inject] protected EnemyController _enemy;
    }
}