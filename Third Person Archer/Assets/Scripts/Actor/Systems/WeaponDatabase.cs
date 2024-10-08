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

        foreach (var weapon in _weapons)
        {
            if (weapon.State == WeaponState.Equipped)
            {
                weapons.Add(weapon);
            }
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

    /*
public WeaponData[] GetWeaponsByID(string[] weaponIDs) 
{
    List<WeaponData> weapons = new List<WeaponData>();

    foreach (var weaponID in weaponIDs)
    {
        foreach (var weaponData in _weapons)
        {
            if (weaponData.ID == weaponID)
            {
                weapons.Add(weaponData);
                break;
            }
        }
    }

    return weapons.ToArray();
}
*/
}
