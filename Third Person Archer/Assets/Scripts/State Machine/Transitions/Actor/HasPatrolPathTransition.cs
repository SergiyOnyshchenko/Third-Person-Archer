using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class HasPatrolPathTransition : StateTransition, IActorIniter
{
    private PatrolPath _path;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out PatrolPath path))
            _path = path;
    }

    public override void Enter()
    {
        base.Enter();

        if (CheckHasPath())
            DoTransition();
    }

    private bool CheckHasPath()
    {
        if(_path == null)
            return false;

        if(!_path.HasPath) 
            return false;

        return true;
    }
}
