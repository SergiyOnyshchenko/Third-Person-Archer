using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class BossStageSubState : SubState
{
    [SerializeField] private int _stageIndex;
    [SerializeField] private ActorController _boss;

    public override void Enter()
    {
        base.Enter();

        SetBossStage();
    }

    public override void Exit() 
    { 
        base.Exit();
    }

    private void SetBossStage()
    {
        if (_boss.TryGetInput(out Stage stage))
        {
            stage.SetStage(_stageIndex);
        }
    }
}
