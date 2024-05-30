using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class InvokeActorActionSubState : SubState, IActorIniter
{
    [SerializeField] private string _enterActionName;
    [SerializeField] private string _exitActionName;
    private ActionInput _actionInput;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out ActionInput actionInput))
            _actionInput = actionInput;
    }

    public override void Enter()
    {
        base.Enter();

        if (!string.IsNullOrEmpty(_enterActionName))
            _actionInput.TryDoAction(_enterActionName);
    }

    public override void Exit()
    {
        if (!string.IsNullOrEmpty(_exitActionName))
            _actionInput.TryDoAction(_exitActionName);

        base.Exit();
    }
}
