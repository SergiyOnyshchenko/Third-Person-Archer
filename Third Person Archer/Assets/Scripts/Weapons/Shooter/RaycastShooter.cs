using System.Collections;
using System.Collections.Generic;
using RootMotion.FinalIK;
using Unity.Burst.CompilerServices;
using UnityEngine;
using UnityEngine.Events;

public class RaycastShooter : Shooter
{
    [SerializeField] private int _damage = 5;
    [SerializeField] private LayerMask _layerMask;

    public override void Shoot(Vector3 direction, float multiplier, UnityAction onHited)
    {
        onHited += SetTargetHitedEvent;

        RaycastHit hit;
        if (Physics.Raycast(_shootPoint.position, direction, out hit, Mathf.Infinity, _layerMask))
        {
            if (hit.transform.TryGetComponent(out IDamageable damager))
            {
                damager.DoDamage(Mathf.RoundToInt(_damage * 1));
                onHited?.Invoke();
            }
        }
    }
}
