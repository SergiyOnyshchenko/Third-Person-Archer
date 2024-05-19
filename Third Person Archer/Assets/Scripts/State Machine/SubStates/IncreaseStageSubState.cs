using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class IncreaseStageSubState : SubState, IActorIniter
{
    private Stage _stage;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out Stage stage))
            _stage = stage;
    }

    public override void Enter()
    {
        base.Enter();
        _stage.IncreaseStage();
    }
}
