using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class OnActorEventTransition : StateTransition, IActorIniter
{
    [SerializeField] private string _eventName;
    private ActorEvent _actorEvent;

    public void InitActor(ActorController actor)
    {
        if (_actorEvent != null)
            return;

        if(actor.TryGetSystem(out EventSystem eventSystem) && 
            eventSystem.TryGetEvent(_eventName, out ActorEvent actorEvent))
            _actorEvent = actorEvent;
    }

    public override void Enter()
    {
        base.Enter();
        _actorEvent.Event.AddListener(DoTransition);
    }

    public override void Exit() 
    {
        _actorEvent.Event.RemoveListener(DoTransition);
        base.Exit();
    }
}
