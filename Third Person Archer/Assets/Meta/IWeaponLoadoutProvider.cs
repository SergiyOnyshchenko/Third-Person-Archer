using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons
{
    public interface IWeaponLoadoutProvider
    {
        float GetEquippedWeaponDamage(WeaponClass weaponClass);
    }
}