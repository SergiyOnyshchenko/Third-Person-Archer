using System.Collections;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;

public class PlayerMoveToPointState : ProcessState, IActorIniter
{
    [SerializeField] private Transform _destination;
    private Transform _target;
    private MoveInput _mover;

    public void InitActor(ActorController actor)
    {
        _target = actor.transform;

        if (actor.TryGetInput(out MoveInput mover))
            _mover = mover;
    }

    public override void Enter()
    {
        base.Enter();

        DOVirtual.DelayedCall(0.1f, () => _mover.MoveToDestination(_destination));
    }
}
