using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Animator = Actor.Animator;

public class PullWeaponAnimationSubState : SubState, IActorIniter
{
    private const string _weaponTypeParamName = "WeaponType";
    private const string _pullWeaponParamName = "PullWeapon";
    private IAnimator _animator;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;
    }

    public override void Enter()
    {
        base.Enter();

        int weaponTypeIndex = 0;
        _animator.SetInteger(_weaponTypeParamName, weaponTypeIndex);
        _animator.SetTrigger(_pullWeaponParamName);
    }
}
