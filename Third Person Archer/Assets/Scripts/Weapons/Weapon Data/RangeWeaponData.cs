using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangeWeaponData : WeaponData
{
    [SerializeField] private int _damage = 100;
    public int Damage { get => _damage; }
}
