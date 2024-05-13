using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public enum WeaponType
    {
        Bow,
        Crossbow,
        Spear
    }

    public class WeaponInventory : System
    {
        [SerializeField] private WeaponType _equippedWeaponType;
        public WeaponType EquippedWeaponType { get => _equippedWeaponType; }
    }
}
