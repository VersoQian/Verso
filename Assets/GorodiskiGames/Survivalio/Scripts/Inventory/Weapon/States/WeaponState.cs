using Game.Core;
using Injection;

namespace Game.Weapon.States
{
    public abstract class WeaponState : State
    {
        [Inject] protected WeaponController _weapon;
    }
}

