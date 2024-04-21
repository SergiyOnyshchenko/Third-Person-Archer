using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class OnFtvTransition : StateTransition, IActorIniter
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
        _cameraPOV.OnFTV.AddListener(DoTransition);
    }

    public override void Exit()
    {
        _cameraPOV.OnFTV.RemoveListener(DoTransition);
        base.Exit();
    }
}
