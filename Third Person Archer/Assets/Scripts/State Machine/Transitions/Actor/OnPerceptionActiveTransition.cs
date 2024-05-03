using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class OnPerceptionActiveTransition : StateTransition, IActorIniter
{
    private PerceptionInput _input;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out PerceptionInput input))
            _input = input;
    }

    private void Update()
    {
        if (_input == null)
            return;

        if(_input.IsActive)
            DoTransition();
    }
}
