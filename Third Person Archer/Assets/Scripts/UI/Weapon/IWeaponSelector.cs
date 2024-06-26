using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public interface IWeaponSelector 
{
    WeaponData WeaponData { get; }  
    event Action<IWeaponSelector> OnWeaponSelected;
    ISelector Selector { get; }
    IEquipper Equipper { get; }
}
