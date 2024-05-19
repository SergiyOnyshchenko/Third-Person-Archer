using System.Collections;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;

public class PlayerAttackStartSubState : SubState, IActorIniter
{
    private AttackInput _attackInput;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;
    }

    public override void Enter()
    {
        base.Enter();

        DOVirtual.DelayedCall(0.1f, () =>
        {
            _attackInput.AllowAttack(true);
        });
    }
}
