using System;

namespace Meta.Weapons
{
    public interface IEquipmentService
    {
        event Action<string> OnEquippedWeaponChanged; // weaponId

        bool Equip(string weaponId);   // returns true if equipped
        bool Unequip(string weaponId); // returns true if unequipped
        string GetEquippedWeaponId(WeaponsState state);
    }
}