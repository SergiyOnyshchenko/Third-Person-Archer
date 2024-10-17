using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Database/Weapons")]
public class WeaponDatabase : ScriptableObject
{
    [SerializeField] private WeaponData[] _weapons;

    public WeaponData[] GetEquippedWeapons()
    {
        List<WeaponData> weapons = new List<WeaponData>();

        foreach (WeaponType type in Enum.GetValues(typeof(WeaponType)))
        {
            WeaponData[] weaponsByType = GetWeaponsByType(type);

            WeaponData equippedWeaponByType = null;

            foreach (var weapon in weaponsByType)
            {
                if (weapon.State == WeaponState.Equipped)
                {
                    if (equippedWeaponByType == null)
                    {
                        equippedWeaponByType = weapon;
                    }
                    else
                    {
                        weapon.Equip(false);
                    }
                }
            }

            if(equippedWeaponByType == null)
            {
                foreach (var weapon in weaponsByType)
                {
                    if (weapon.State == WeaponState.Unlocked)
                    {
                        equippedWeaponByType = weapon;
                        weapon.Equip(true);
                        break;
                    }
                }
            }

            if (equippedWeaponByType != null)
                weapons.Add(equippedWeaponByType);
        }

        return weapons.ToArray();
    }

    public void Save()
    {
        foreach (var weapon in _weapons)
        {
            weapon.Save();
        }
    }

    public void Load()
    {
        foreach (var weapon in _weapons)
        {
            weapon.Load();
        }
    }

    public WeaponData[] GetWeaponsByType(WeaponType type)
    {
        List<WeaponData> weapons = new List<WeaponData>();

        foreach (var weapon in _weapons)
        {
            if (weapon.Type == type)
            {
                weapons.Add(weapon);
            } 
        }

        return weapons.ToArray();
    }
}
