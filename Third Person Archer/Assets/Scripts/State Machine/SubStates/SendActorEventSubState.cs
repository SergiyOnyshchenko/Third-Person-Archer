using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class SendActorEventSubState : SubState, IActorIniter
{
    [SerializeField] private string _name;
    [SerializeField] private StatePlacement _placement;
    private EventSystem _eventSystem;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out EventSystem eventSystem))
            _eventSystem = eventSystem;
    }

    public override void Enter()
    {
        base.Enter();

        if (_placement == StatePlacement.OnEnter)
            _eventSystem.TryInvokeEvent(_name);
    }

    public override void Exit() 
    {
        if (_placement == StatePlacement.OnExit)
            _eventSystem.TryInvokeEvent(_name);

        base.Exit();
    }
}
