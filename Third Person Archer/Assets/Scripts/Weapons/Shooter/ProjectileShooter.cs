using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ProjectileShooter : Shooter
{
    [SerializeField] private Projectile _prefab;

    public override void Shoot(Vector3 direction, float multiplier)
    {
        Projectile arrow = Instantiate(_prefab, _shootPoint.position, _shootPoint.rotation);
        arrow.Shoot(direction, multiplier);
    }
}
