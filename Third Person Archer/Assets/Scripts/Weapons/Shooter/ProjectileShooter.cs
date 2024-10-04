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

        RaycastHit hit;

        if (Physics.Raycast(_aimInput.GetAimRoot(), direction, out hit, Mathf.Infinity, arrow.HitLayers))
        {
            direction = (hit.point - _shootPoint.position).normalized;
        }
        else
        {
            direction = (PointAlongDirection(_aimInput.GetAimRoot(), direction, 100f) - _shootPoint.position).normalized;
        }

        arrow.Shoot(direction, multiplier, onHited);

        OnShooted?.Invoke(arrow);
    }

   private Vector3 PointAlongDirection(Vector3 origin, Vector3 direction, float distance)
   {
        return origin + direction.normalized * distance;
   }

    public void SetProjectile(Projectile projectile)
    {
        _prefab = projectile;
    }
}
