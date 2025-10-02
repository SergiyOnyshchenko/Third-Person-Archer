using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class DestroyActorSubstate : SubState, IActorIniter
{
    private ActorController _actor;

    public void InitActor(ActorController actor)
    {
        _actor = actor;
    }

    public override void Enter()
    {
        base.Enter();
        Destroy(_actor.gameObject);
    }
}