using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ProjectileHitedTransition : StateTransition, IActorIniter
{
    private ProjectileHitHandler _hitHandler;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _hitHandler)) { }
    }

    public override void Enter()
    {
        base.Enter();

        if (_hitHandler != null)
            _hitHandler.OnHited.AddListener(DoTransition);
    }

    public override void Exit()
    {
        if (_hitHandler != null)
            _hitHandler.OnHited.RemoveListener(DoTransition);

        base.Exit();
    }
}