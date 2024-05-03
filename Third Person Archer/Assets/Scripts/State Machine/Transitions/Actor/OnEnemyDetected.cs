using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class OnEnemyDetected : StateTransition, IActorIniter
{
    private PerceptionInput _input;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out PerceptionInput input))
            _input = input;
    }

    private void Update()
    {
        if (_input.Target != null)
            DoTransition();
    }
}
