using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public abstract class RangeWeaponData : WeaponData
{
    [SerializeField] private int _damage = 100;
    public int Damage { get => _damage; }
}
