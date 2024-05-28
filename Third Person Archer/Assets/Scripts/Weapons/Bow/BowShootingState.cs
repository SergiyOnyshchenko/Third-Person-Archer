using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAnimation.FPV;
using CustomAnimation.Body;
using DG.Tweening;
using Actor;
using Input = UnityEngine.Input;

public class BowShootingState : ProcessState, IActorIniter
{
    [SerializeField] private Projectile _projectile;
    [SerializeField] private Transform _directionCamera;
    [SerializeField] private GameObject _bowArrow;
    private BowController _bowController;
    private AttackInput _attackInput;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out BowController bow))
            _bowController = bow;

        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;
    }

    public override void Enter()
    {
        base.Enter();

        _bowController.SetStartSettings();
        _attackInput.OnAttackRelease.AddListener(PullArrow);
    }

    public override void Exit() 
    {
        _attackInput.OnAttackRelease.RemoveListener(PullArrow);
        base.Exit();
    }

    private void Update()
    {
        if (_attackInput.IsHold)
        {
            if(!_bowController.IsPulling)
                _bowController.BeginPull();

            _bowController.HoldPull();
        }
    }

    private void PullArrow()
    {
        _bowController.ReleasePull();
        FinishProcess();
    }
    
    /*
    private void ShootProjectile()
    {
        Projectile arrow = Instantiate(_projectile, _bowArrow.transform.position, _bowArrow.transform.rotation);
        arrow.Shoot(_directionCamera.forward, _bowController.PullPower, null);
    }
    */
}
