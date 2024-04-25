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

    public override void Exit()
    {
        _mover.Stop();
        base.Exit();
    }

    private void Update()
    {
        if (_stop)
            return;

        float distance = Vector3.Distance(_target.position, _destination.position);

        if (distance < 0.1f)
        {
            _stop = true;
            DOVirtual.DelayedCall(0.1f, FinishProcess);
        }
    }
}
