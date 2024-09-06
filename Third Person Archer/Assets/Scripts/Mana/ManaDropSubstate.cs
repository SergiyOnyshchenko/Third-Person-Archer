using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class ManaDropSubstate : SubState, IActorIniter
{
    private ManaDropper _manaDropper;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out ManaDropper manaDropper))
            _manaDropper = manaDropper;
    }

    public override void Enter()
    {
        base.Enter();

        _manaDropper.Drop();
    }
}
