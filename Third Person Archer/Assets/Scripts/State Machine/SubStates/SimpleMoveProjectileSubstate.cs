using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class SimpleMoveProjectileSubstate : SubState, IActorIniter
{
    private ProjectileDirection _direction;
    private Actor.ProjectileMover _mover;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _mover)) { }
        if (actor.TryGetProperty(out _direction)) { }
    }

    public void FixedUpdate()
    {
        _mover.Move(_direction.Value);
    }
}
