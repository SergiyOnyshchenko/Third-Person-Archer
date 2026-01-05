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

        if (_shootError != null)
        {
            float horizontalAngle = Random.Range(-_shootError.Value.x, _shootError.Value.x);
            float verticalAngle = Random.Range(-_shootError.Value.y, _shootError.Value.y);

            Quaternion horizontalRotation = Quaternion.AngleAxis(horizontalAngle, Vector3.up);
            Vector3 horizontalRotated = horizontalRotation * direction;

            Vector3 right = Vector3.Cross(Vector3.up, horizontalRotated);
            if (right == Vector3.zero)
                right = Vector3.right;

            Quaternion verticalRotation = Quaternion.AngleAxis(verticalAngle, right);
            Vector3 finalDirection = verticalRotation * horizontalRotated;

            direction = finalDirection.normalized;
        }

        projectile.Shoot(direction, multiplier, onHited);
    }
}