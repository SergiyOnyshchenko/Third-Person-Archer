using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using UnityEngine.Windows;

public class BeginShootingTrasnition : StateTransition, IActorIniter
{
    protected ShootingInput _input;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out ShootingInput input))
            _input = input;
    }

    private void Update()
    {
        if (_input == null)
            return;

        if (_input.IsActive)
            DoTransition();
    }
}
