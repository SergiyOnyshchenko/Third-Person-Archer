using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class PlayerMoveToPointsSequenceState : ProcessState, IActorIniter
{
    [SerializeField] private Transform[] _destinations;
    private int _currentDestinationIndex = -1;
    private MoveInput _mover;
    public Transform CurrentDestinationPoint { get => _destinations[_currentDestinationIndex];  }

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out MoveInput mover))
            _mover = mover;
    }

    public override void Enter()
    {
        base.Enter();

        _currentDestinationIndex++;

        if(_currentDestinationIndex >= _destinations.Length)
            _currentDestinationIndex = 0;

        _mover.MoveToDestination(CurrentDestinationPoint);
    }
}
