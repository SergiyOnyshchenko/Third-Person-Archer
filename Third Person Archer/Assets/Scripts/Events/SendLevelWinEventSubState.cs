using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using MoreMountains.Tools;
using UnityEngine;

public class SendLevelWinEventSubState : SubState, IActorIniter
{
    private ActorController _actor;
    public void InitActor(ActorController actor)
    {
        _actor = actor;
    }

    public override void Enter()
    {
        base.Enter();

        YsoCorp.GameUtils.YCManager.instance.OnGameFinished(true);
    }  
}
