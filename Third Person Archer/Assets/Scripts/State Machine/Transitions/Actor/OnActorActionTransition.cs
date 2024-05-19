using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class OnActorActionTransition : StateTransition, IActorIniter
{
    [SerializeField] private string _actionName;
    private ActorAction _actorAction;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetInput(out ActionInput actionInput) && actionInput.TryGetAction(_actionName, out ActorAction actorAction))
        {
            _actorAction = actorAction;
        }
    }

    public override void Enter()
    {
        base.Enter();

        _actorAction.OnAction.AddListener(DoTransition);
    }

    public override void Exit()
    {
        _actorAction.OnAction.RemoveListener(DoTransition);

        base.Exit();
    }
}
