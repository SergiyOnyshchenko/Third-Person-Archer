using System.Collections;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class SpearThrowState : ProcessState, IActorIniter
{
    private SpearController _spearController;
    private AttackInput _attackInput;

    public UnityEvent OnThrow = new UnityEvent();

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out SpearController spear))
            _spearController = spear;

        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;
    }

    public override void Enter()
    {
        base.Enter();

        _spearController.SetStartSettings();
        _attackInput.OnAttackRelease.AddListener(PullArrow);
    }

    public override void Exit() 
    { 
        base.Exit();
        _attackInput.OnAttackRelease.RemoveListener(PullArrow);
    }

    private void Update()
    {
        if (_attackInput.IsHold)
        {
            if (!_spearController.IsPulling)
                _spearController.BeginPull();

            _spearController.HoldPull();
        }
    }

    private void PullArrow()
    {
        _spearController.ReleasePull();
        OnThrow?.Invoke();

        DOVirtual.DelayedCall(0.5f, FinishProcess);
    }
}
