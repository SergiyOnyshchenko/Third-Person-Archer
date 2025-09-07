using Actor;
using Meta.Weapons;

namespace Game.Weapons
{
    /// <summary>Equip a weapon on an ActorController (spawn already handled elsewhere if needed).</summary>
    public interface IGameplayEquipper
    {
        /// <summary>Returns true if this equipper handled the provided weapon class.</summary>
        bool TryEquip(ActorController actor, WeaponDef definition, WeaponStats stats);
    }
}