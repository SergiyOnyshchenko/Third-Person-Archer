using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ActorMoveSequence : Sequence, ITargetActor
{
    [SerializeField] private Transform _destination;
    public ActorController TargetActor { get; set; }

    public override void Begin()
    {
        base.Begin();

        if(TargetActor.TryGetInput(out MoveInput mover))
        {
            mover.MoveToDestination(_destination);
        }
    }
}
