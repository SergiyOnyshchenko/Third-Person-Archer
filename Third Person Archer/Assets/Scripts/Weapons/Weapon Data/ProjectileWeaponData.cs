using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ProjectileWeaponData : RangeWeaponData
{
    [SerializeField] private Projectile _projectile;
    public Projectile Projectile { get => _projectile; }
}
