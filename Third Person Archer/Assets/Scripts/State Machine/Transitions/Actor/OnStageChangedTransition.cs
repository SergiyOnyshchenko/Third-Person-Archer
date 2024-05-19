using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class OnStageChangedTransition : StateTransition, IActorIniter
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

        _stage.OnStageStarted.AddListener(CheckStage);
        CheckStage(_stage.StageIndex);
    }

    public override void Exit()
    {
        _stage.OnStageStarted.RemoveListener(CheckStage);

        base.Exit();
    }

    private void CheckStage(int stage)
    {
        DoTransition();
    }
}
