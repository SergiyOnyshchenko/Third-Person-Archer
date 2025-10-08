using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ProjectileShooter : Shooter, IActorIniter
{
    [Header("Weapon")]
    [SerializeField] private Projectile _prefab;
    public Projectile Prefab => _prefab;
    public UnityEvent<Projectile> OnShooted = new UnityEvent<Projectile>();

    public void SetProjectile(Projectile projectile)
    {
        _prefab = projectile;
    }

    public override void Shoot(Vector3 direction, float multiplier, UnityAction onHited)
    {
        onHited += SetTargetHitedEvent;

        Projectile projectile = Instantiate(Prefab, _shootPoint.position, _shootPoint.rotation);
        projectile.Shoot(direction, multiplier, onHited);
    }
}