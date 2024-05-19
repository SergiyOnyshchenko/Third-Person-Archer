using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class SpeedModifierSubState : SubState, IActorIniter
{
    [SerializeField] private float _newSpeed = 5;
    [SerializeField] private bool _resetOnExit = true;
    private Speed _speed;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetProperty(out Speed speed))
            _speed = speed;
    }

    public override void Enter()
    {
        base.Enter();
        _speed.SetValue(_newSpeed);
    }

    public override void Exit()
    {
        if(_resetOnExit)
            _speed.ResetValue();

        base.Exit();
    }
}
