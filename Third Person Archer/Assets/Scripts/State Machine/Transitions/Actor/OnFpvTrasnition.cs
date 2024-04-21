using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class OnFpvTrasnition : StateTransition, IActorIniter
{
    private CameraPOV _cameraPOV;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out CameraPOV pov))
            _cameraPOV = pov;
    }

    public override void Enter()
    {
        base.Enter();
        _cameraPOV.OnFPV.AddListener(DoTransition);
    }

    public override void Exit() 
    {
        _cameraPOV.OnFPV.RemoveListener(DoTransition);
        base.Exit();
    }
}
