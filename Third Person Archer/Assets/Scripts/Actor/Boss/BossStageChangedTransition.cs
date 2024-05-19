using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class BossStageChangedTransition : StateTransition
{
    [SerializeField] private int _targetStage;
    [SerializeField] private ActorController _boss;
    private Stage _stage;

    public override void Enter()
    {
        base.Enter();

        if (_stage == null && _boss.TryGetInput(out Stage stage))
            _stage = stage;

        _stage.OnStageStarted.AddListener(CheckStage);
    }

    public override void Exit()
    {
        _stage.OnStageStarted.RemoveListener(CheckStage);
        base.Exit();
    }

    private void CheckStage(int stage)
    {
        if (_targetStage == stage)
            DoTransition();
    }
}

