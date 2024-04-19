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
            mover.MoveToDestination(_destination);
    }

    protected override void Finish()
    {
        if (TargetActor.TryGetInput(out MoveInput mover))
            mover.Stop();

        base.Finish();
    }

    private void Update()
    {
        float distance = Vector3.Distance(TargetActor.transform.position, _destination.position);

        if (distance < 0.1f)
        {
            Finish();
        }
    }
}
