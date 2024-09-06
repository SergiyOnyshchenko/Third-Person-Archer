using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ProjectileShooter : Shooter
{
    [SerializeField] private Projectile _prefab;
    public UnityEvent<Projectile> OnShooted = new UnityEvent<Projectile>();

    public override void Shoot(Vector3 direction, float multiplier, UnityAction onHited)
    {
        onHited += SetTargetHitedEvent;

        Projectile arrow = Instantiate(_prefab, _shootPoint.position, _shootPoint.rotation);

        if(_elementalAttackType != null)
            arrow.SetElementalType(_elementalAttackType.Value);

        arrow.Shoot(direction, multiplier, onHited);

        OnShooted?.Invoke(arrow);
    }

    public void SetProjectile(Projectile projectile)
    {
        _prefab = projectile;
    }
}
