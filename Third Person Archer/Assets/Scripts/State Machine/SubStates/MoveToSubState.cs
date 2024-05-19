using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class MoveToSubState : SubState, IActorIniter
{
    [SerializeField] private Transform _targetPosition;
    private Mover _mover;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out Mover mover))
            _mover = mover;
    }

    public override void Enter()
    {
        base.Enter();
        _mover.Move(_targetPosition, null);
    }
}
