using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public abstract class CamerPovState : SubState, IActorIniter
{
    protected CameraPOV _cameraPOV;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out CameraPOV cameraPOV))
            _cameraPOV = cameraPOV;
    }

    public override void Enter()
    {
        base.Enter();
        SetCamera();
    }

    public abstract void SetCamera();
}
