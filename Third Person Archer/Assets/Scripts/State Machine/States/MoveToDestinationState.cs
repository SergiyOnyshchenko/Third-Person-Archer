using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class MoveToDestinationState : MainState, IActorIniter
{
    private IMoveInput _input;
    private Mover _mover;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out MoveInput input))
            _input = input;

        if (actor.TryGetSystem(out Mover mover))
            _mover = mover;
    }

    public override void Enter()
    {
        base.Enter();

        _mover.Move(_input.MovePostion);
    }
}
