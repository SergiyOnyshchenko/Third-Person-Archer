using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class GuidedProjectileSubstate : SubState, IActorIniter
{
    private ProjectileDirection _direction;
    private GuidedProjectileMover _mover;
    private FpvInput _input;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _mover)) { }
        if (actor.TryGetProperty(out _direction)) { }
        if (actor.TryGetInput(out _input)) { }
    }

    public void FixedUpdate()
    {
        _mover.Move(_input.Horizontal, _input.Vertical);
    }
}
