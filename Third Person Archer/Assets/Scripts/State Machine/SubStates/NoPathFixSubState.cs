using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class NoPathFixSubState : SubState, IActorIniter
{
    [SerializeField] private float _checkDelay = 1;
    private float _timer;
    private NavmeshMover _mover;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out NavmeshMover mover))
            _mover = mover;
    }

    private void Update()
    {
        if (_mover.Velocity.magnitude == 0)
        {
            _timer += Time.deltaTime;

            if(_timer >= _checkDelay)
            {
                _mover.FinishMovingManualy();
                _timer = 0;
            }
        }
        else
        {
            _timer = 0;
        }
    }
}
