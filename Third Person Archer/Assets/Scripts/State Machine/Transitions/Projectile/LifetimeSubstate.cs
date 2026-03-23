using Actor;
using UnityEngine;

public class LifetimeSubstate : SubState, IActorIniter
{
    private Actor.Lifetime _lifetime;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out _lifetime)){}
    }

    private void Update()
    {
        _lifetime.Tick();
    }
}