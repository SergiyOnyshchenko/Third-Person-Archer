using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDatabase", menuName = "Database/Weapons")]
public class WeaponDatabase : ScriptableObject
{
    [SerializeField] private WeaponData[] _weapons;

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
}
