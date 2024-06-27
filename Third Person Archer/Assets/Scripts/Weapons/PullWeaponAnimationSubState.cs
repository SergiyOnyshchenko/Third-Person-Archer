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
    private WeaponInventory _weaponInventory;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;

        if (actor.TryGetSystem(out WeaponInventory inventory))
            _weaponInventory = inventory;
    }

    public override void Enter()
    {
        base.Enter();

        int weaponTypeIndex = (int)_weaponInventory.EquippedWeaponType;
        _animator.SetInteger(_weaponTypeParamName, weaponTypeIndex);
        _animator.SetTrigger(_pullWeaponParamName);
    }
}
