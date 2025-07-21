using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Actor;
using UnityEngine;

public class SendLevelLoseEventSubState : SubState, IActorIniter
{
    private ActorController _actor;
    public void InitActor(ActorController actor)
    {
        _actor = actor;
    }

    public override void Enter()
    {
        base.Enter();

        YsoCorp.GameUtils.YCManager.instance.OnGameFinished(false);
    }
}
