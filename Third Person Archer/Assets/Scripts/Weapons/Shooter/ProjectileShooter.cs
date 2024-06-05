using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using EventSystem = Actor.EventSystem;

public class ProjectileShooter : Shooter
{
    [SerializeField] private Projectile _prefab;
    public UnityEvent OnShooted = new UnityEvent();

    public override void Shoot(Vector3 direction, float multiplier, UnityAction onHited)
    {
        onHited += SetTargetHitedEvent;

        Projectile arrow = Instantiate(_prefab, _shootPoint.position, _shootPoint.rotation);
        arrow.Shoot(direction, multiplier, onHited);

        OnShooted?.Invoke();
    }
}
