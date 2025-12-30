using System.Collections;
using System.Collections.Generic;
using Meta.Weapons;
using UnityEngine;

public class PlayerWeaponEquipDirty : MonoBehaviour
{
    private PlayerWeaponBinder _playerWeaponBinder;

    public void Equip(WeaponDef weapon)
    {
        if(_playerWeaponBinder == null)
            _playerWeaponBinder = FindAnyObjectByType<PlayerWeaponBinder>();

        if(_playerWeaponBinder == null)
            return;
        
        _playerWeaponBinder.EquipWeaponWithoutStats(weapon);
    }
}
