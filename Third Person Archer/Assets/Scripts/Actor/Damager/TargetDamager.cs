using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class TargetDamager : MonoBehaviour, IActorIniter
{
    [SerializeField] private int _damage;
    [Tooltip("The blow misses a target further away than this when it lands. 0 = any distance.")]
    [SerializeField, Min(0f)] private float _maxDistance;
    private PerceptionInput _perceptionInput;
    private Transform _transform;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;

        if(actor.TryGetInput(out PerceptionInput perceptionInput))
            _perceptionInput = perceptionInput;
    }

    public void DoDamage()
    {
        if (_perceptionInput == null)
            return;

        if (_perceptionInput.Target == null)
            return;

        if (_maxDistance > 0f && Vector3.Distance(_perceptionInput.Target.RootPoint.position, _transform.position) > _maxDistance)
            return;

        _perceptionInput.Target.Damageable.DoDamage(_damage);
    }
}
