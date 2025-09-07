using Meta.Weapons;
using UnityEngine;

namespace Game.Weapons
{
    public interface IProjectileResolver
    {
        /// <summary>Return a projectile prefab (or component root) for the given weapon.</summary>
        Projectile ResolveProjectile(WeaponDef definition);
    }
}
