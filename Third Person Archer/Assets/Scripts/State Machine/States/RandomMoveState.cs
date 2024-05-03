using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class RandomMoveState : ProcessState, IActorIniter
{
    [SerializeField] private float _range = 5f;
    private Transform _transform;
    private MoveInput _moverInput;
    private Vector3 _startPosition;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;
        _startPosition = _transform.position;

        if (actor.TryGetInput(out MoveInput mover))
            _moverInput = mover;
    }

    public override void Enter()
    {
        base.Enter();
        MoveRandom();
    }

    public void MoveRandom()
    {
        Vector3 movePosition = new Vector3(Random.Range(-_range, _range), 0, Random.Range(-_range, _range));
        movePosition += _startPosition;

        //_mover.Move(movePosition, FinishProcess);

        _moverInput.MoveToDestination(movePosition);
    }
}
