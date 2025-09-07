using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAmmoCountView
{
    Meta.Weapons.WeaponClass WeaponType { get; }
    GameObject gameObject { get; }
}
