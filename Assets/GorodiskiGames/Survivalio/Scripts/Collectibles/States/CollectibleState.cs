using Game.Core;
using Injection;

namespace Game.Collectible.States
{
    public abstract class CollectibleState : State
    {
        [Inject] protected CollectibleController _collectible;
    }
}

