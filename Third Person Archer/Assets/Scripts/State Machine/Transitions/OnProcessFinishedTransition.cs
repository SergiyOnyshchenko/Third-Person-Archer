using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnProcessFinishedTransition : StateTransition
{
    private ProcessState _state;

    private void Awake()
    {
        _state = GetComponent<ProcessState>();
    }

    public override void Enter()
    {
        base.Enter();
        _state.OnProcessFinished.AddListener(DoTransition);
    }

    public override void Exit()
    {
        _state.OnProcessFinished.RemoveListener(DoTransition);
        base.Exit();
    }
}
