using System.Collections;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;

public abstract class WeaponThrowState : ProcessState, IActorIniter
{
    protected ThrowWeaponController _weaponController;
    private AttackInput _attackInput;
    private WeaponPull _weaponPull;
    private float _pullThreshold = 0.5f;

    protected abstract void InitWeaponController(ActorController actor);

    public virtual void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;

        if (actor.TryGetProperty(out WeaponPull weaponPull))
            _weaponPull = weaponPull;

        InitWeaponController(actor);
    }

    public override void Enter()
    {
        base.Enter();

        _weaponController.SetStartSettings();
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
            if (!_weaponController.IsPulling)
                _weaponController.BeginPull();

            _weaponController.HoldPull();
        }
    }

    private void PullArrow()
    {
        if (_weaponPull.Value < _pullThreshold)
            return;

        _weaponController.ReleasePull();
        DOVirtual.DelayedCall(0.5f, FinishProcess);
    }
}
