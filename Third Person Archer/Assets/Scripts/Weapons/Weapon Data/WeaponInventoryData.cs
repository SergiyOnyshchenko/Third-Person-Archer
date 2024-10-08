using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[CreateAssetMenu(fileName = "WeaponInventory", menuName = "Data/Inventory/Weapons")]
public class WeaponInventoryData : ScriptableObject
{
    [SerializeField] private string _name;
    [SerializeField] private WeaponData[] _weapons;
    public WeaponData[] Weapons { get => _weapons; }
    public event Action<WeaponData> OnEquipped;

    public void Init(WeaponData[] weapons)
    {
        _weapons = weapons; 
    }

    public WeaponData GetWeaponByType(WeaponType type)
    {
        foreach (var weapon in _weapons)
            if (weapon.Type == type)
                return weapon;

        return null;
    }

    public void EquipWeapon(WeaponData newWeapon)
    {
        newWeapon.Equip(true);
        OnEquipped?.Invoke(newWeapon);
    }

    public void AddNewWeapon(WeaponData newWeapon)
    {
        for (int i = 0; i < _weapons.Length; i++)
        {
            if (_weapons[i].Type == newWeapon.Type)
            {
                _weapons[i].Equip(false);

                _weapons[i] = newWeapon;

                _weapons[i].Equip(true);

                SendEquipEvents(newWeapon);

                return;
            }
        }

        WeaponData[] newArray = new WeaponData[_weapons.Length + 1];

        for (int i = 0; i < _weapons.Length; i++)
            newArray[i] = _weapons[i];

        newArray[newArray.Length - 1] = newWeapon;
        SendEquipEvents(newWeapon);

        _weapons = newArray;
    }

    private void SendEquipEvents(WeaponData newWeapon)
    {

    }
}
