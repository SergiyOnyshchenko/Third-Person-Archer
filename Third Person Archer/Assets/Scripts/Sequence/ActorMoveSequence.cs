using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using DG.Tweening;

public class ActorMoveSequence : Sequence, ITargetActor
{
    [SerializeField] private Transform _destination;
    private bool _stop;
    public ActorController TargetActor { get; set; }

    public override void Begin()
    {
        base.Begin();

        _stop = false;

        if (TargetActor.TryGetInput(out MoveInput mover))
            mover.MoveToDestination(_destination);
    }

    protected override void Finish()
    {
        //if (TargetActor.TryGetInput(out MoveInput mover))
        //    mover.Stop();

        base.Finish();
    }

    private void Update()
    {
        if (_stop)
            return;

        float distance = Vector3.Distance(TargetActor.transform.position, _destination.position);

        if (distance < 0.1f)
        {
            _stop = true;
            DOVirtual.DelayedCall(0.1f, Finish);
        }
    }
}
