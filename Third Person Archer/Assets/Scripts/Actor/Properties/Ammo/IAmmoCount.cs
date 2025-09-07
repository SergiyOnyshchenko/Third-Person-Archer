using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.Weapons;

public interface IAmmoCount
{
    int AmmoCount { get; }
    WeaponClass WeaponType { get; }
}
