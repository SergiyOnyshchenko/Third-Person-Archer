using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMeleeState : AnimationAttackState
{
    [SerializeField] private TargetDamager _damager;

    protected override void Attack()
    {
        base.Attack();
        _damager.DoDamage();
    }
}
