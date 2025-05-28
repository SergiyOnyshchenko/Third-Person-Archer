using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class SetProjectileStateSubstate : SubState, IActorIniter
{
    [SerializeField] private StatePlacement _placement;
    [SerializeField] private ProjectileState _state;
    private ProjectileStateProperty _stateProperty;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetProperty(out _stateProperty)) { }
    }

    public override void Enter()
    {
        base.Enter();

        if (_placement == StatePlacement.OnEnter)
            _stateProperty.SetValue(_state);
    }

    public override void Exit()
    {
        if (_placement == StatePlacement.OnExit)
            _stateProperty.SetValue(_state);

        base.Exit();
    }
}
