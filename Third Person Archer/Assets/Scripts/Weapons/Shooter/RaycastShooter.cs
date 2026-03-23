using System.Collections;
using System.Collections.Generic;
using Actor;
using RootMotion.FinalIK;
using UnityEngine;
using UnityEngine.Events;

public class RaycastShooter : Shooter
{
    [SerializeField] private int _damage = 5;
    [SerializeField] private LayerMask _layerMask;

    public override void Shoot(Vector3 direction, float multiplier,
        UnityAction<ActorController> onTargetHited, UnityAction onAnyHit)
    {
        RaycastHit hit;
        if (Physics.Raycast(_shootPoint.position, direction, out hit, Mathf.Infinity, _layerMask))
        {
            if (hit.transform.TryGetComponent(out IDamageable damageable))
            {
                damageable.DoDamage(Mathf.RoundToInt(_damage * multiplier));

                ActorController actor = null;
                if (hit.transform.TryGetComponent(out IActorOwner actorOwner))
                    actor = actorOwner.Actor;

                onTargetHited?.Invoke(actor);
                SetTargetHitedEvent();
            }

            onAnyHit?.Invoke();
        }
        else
        {
            onAnyHit?.Invoke();
        }
    }
}