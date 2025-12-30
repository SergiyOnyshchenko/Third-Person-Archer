using System;

namespace Meta.Weapons
{
    public interface IEquipmentService
    {
        /// <summary>Fired when a slot changes: (class, newWeaponId).</summary>
        event Action<WeaponClass, string> OnEquippedWeaponChanged;

        bool Equip(WeaponClass cls, string weaponId);
        string GetEquippedWeaponId(WeaponsState state, WeaponClass cls);
        string GetEquippedWeaponId(WeaponClass cls);
    }
}