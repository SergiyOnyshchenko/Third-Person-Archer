using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class ActorJumpSequence : Sequence, ITargetActor
{
    [SerializeField] private Spline _jumpSpline;
    public ActorController TargetActor { get; set; }

    public override void Begin()
    {
        base.Begin();

        if (TargetActor.TryGetInput(out JumpInput jumper))
            jumper.Jump(_jumpSpline);
    }
}
