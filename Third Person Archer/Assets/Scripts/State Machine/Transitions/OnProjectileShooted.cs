using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class OnProjectileShooted : StateTransition, IActorIniter
{
    private Projectile _projectile;

    public void InitActor(ActorController actor)
    {
        _projectile = actor.GetComponent<Projectile>();
    }

    public override void Enter()
    {
        base.Enter();

        if (_projectile != null)
            _projectile.OnShooted.AddListener(DoTransition);
    }

    public override void Exit()
    {
        if (_projectile != null)
            _projectile.OnShooted.RemoveListener(DoTransition);

        base.Exit();
    }
}

