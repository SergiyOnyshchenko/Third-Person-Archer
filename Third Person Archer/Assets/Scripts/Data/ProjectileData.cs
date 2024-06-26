using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ProjectileData : Data
{
    [SerializeField] private Projectile _value;
    public Projectile Value { get => _value; }
}
