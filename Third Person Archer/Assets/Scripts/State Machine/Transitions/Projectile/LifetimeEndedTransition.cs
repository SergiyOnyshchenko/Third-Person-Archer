using Actor;
using UnityEngine;

public class LifetimeEndedTransition : StateTransition, IActorIniter
{
    private Actor.Lifetime _lifetime;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out _lifetime)){}
    }

    public override void Enter()
    {
        base.Enter();
        _lifetime.OnLifetimeEnded.AddListener(DoTransition);

        if(_lifetime.IsLifetimeEnded)
            DoTransition();
    }

    public override void Exit()
    {
        _lifetime.OnLifetimeEnded.AddListener(DoTransition);
        base.Exit();
    }
}