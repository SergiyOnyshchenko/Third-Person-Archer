using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.ThirdPerson;
using WeaponController = Actor.ThirdPerson.WeaponController;

public class ShootingState : MainState, IActorIniter
{
    private AimInput _aimer;
    private IAnimator _animator;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out AimInput aimer))
            _aimer = aimer;

        if (actor.TryGetSystem(out Actor.Animator animator))
            _animator = animator;
    }

    public override void Enter()
    {
        base.Enter();

        _animator.SetTrigger("Shoot"); 
    }
}
