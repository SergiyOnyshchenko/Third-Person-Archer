using System.Collections;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;

public class PlayerMoveToPointState : ProcessState, IActorIniter
{
    [SerializeField] private Transform _destination;
    private bool _stop;
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

        _stop = false;
        _mover.MoveToDestination(_destination);
    }
}
