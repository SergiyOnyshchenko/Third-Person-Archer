using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAmmoCount
{
    int AmmoCount { get; }
    WeaponType WeaponType { get; }
}
