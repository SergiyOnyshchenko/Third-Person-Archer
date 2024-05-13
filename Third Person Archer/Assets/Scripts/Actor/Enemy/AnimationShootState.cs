using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Animator = Actor.Animator;

[RequireComponent(typeof(Shooter))]
public class AnimationShootState : AnimationAttackState
{
    private AimInput _aim;
    private Shooter _shooter;

    public override void InitActor(ActorController actor)
    {
        base.InitActor(actor);

        if (actor.TryGetInput(out AimInput aim))
            _aim = aim;

        _shooter = GetComponent<Shooter>();
    }

    protected override void Attack()
    {
        base.Attack();
        _shooter.Shoot(_aim.GetAimDirection(), 1f);
    }
}
