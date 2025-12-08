using System;

namespace Meta.Weapons
{
    public interface IEquipmentService
    {
        /// <summary>Fired when a slot changes: (class, newWeaponId).</summary>
        event Action<WeaponClass, string> OnEquippedWeaponChanged;

        bool Equip(WeaponClass cls, string weaponId);
        string GetEquippedWeaponId(WeaponsState state, WeaponClass cls);

        /// <summary>Returns the equipped weapon id for the given class using saved state.</summary>
        string GetEquippedWeaponId(WeaponClass cls);
    }
}