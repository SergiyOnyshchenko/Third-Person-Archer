using System.Collections;
using System.Collections.Generic;
using Actor;
using Cinemachine.Utility;
using UnityEngine;

public class PlayerJumpState : MainState, IActorIniter
{
    [SerializeField] private Spline _jumpSpline;
    private JumpInput _jumper;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out JumpInput jumper))
            _jumper = jumper;
    }

    public override void Enter()
    {
        base.Enter();
        _jumper.Jump(_jumpSpline);
    }
}
