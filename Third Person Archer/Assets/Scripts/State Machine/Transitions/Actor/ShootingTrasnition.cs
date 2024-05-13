using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public abstract class ShootingTrasnition : StateTransition, IActorIniter
{
    protected AttackInput _input;

    public abstract bool CheckTransition();

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out AttackInput input))
            _input = input;
    }

    private void Update()
    {
        if (_input == null)
            return;

        if (CheckTransition())
            DoTransition();
    }
}
