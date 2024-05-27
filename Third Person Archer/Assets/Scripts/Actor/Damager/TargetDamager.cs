using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class TargetDamager : MonoBehaviour, IActorIniter
{
    [SerializeField] private int _damage;
    private PerceptionInput _perceptionInput;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetInput(out PerceptionInput perceptionInput))
            _perceptionInput = perceptionInput;
    }

    public void DoDamage()
    {
        if (_perceptionInput == null)
            return;

        if (_perceptionInput.Target == null)
            return;

        _perceptionInput.Target.Damageable.DoDamage(_damage);
    }
}
