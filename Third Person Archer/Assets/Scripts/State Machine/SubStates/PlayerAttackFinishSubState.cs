using System.Collections;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;

public class PlayerAttackFinishSubState : SubState, IActorIniter
{
    public enum Placement
    {
        EnterState,
        ExitState
    }

    [SerializeField] private Placement _placement;
    private AttackInput _attackInput;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;
    }

    public override void Enter()
    {
        base.Enter();

        if (_placement == Placement.EnterState)
            _attackInput.AllowAttack(false);
    }

    public override void Exit()
    {
        if (_placement == Placement.ExitState)
            _attackInput.AllowAttack(false);

        base.Exit();
    }
}
