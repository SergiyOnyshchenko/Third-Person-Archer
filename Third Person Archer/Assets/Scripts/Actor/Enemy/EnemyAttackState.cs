using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class EnemyAttackState : MainState, IActorIniter
{
    private EnemyAttackInput _attackInput;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetInput(out EnemyAttackInput attackInput))
            _attackInput = attackInput;
    }

    public override void Enter()
    {
        base.Enter();
        _attackInput.AllowAttack(true);
    }

    public override void Exit()
    {
        _attackInput.AllowAttack(false);
        base.Exit();
    }
}
