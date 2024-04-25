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

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out BowController bow))
            _bowController = bow;
    }

    public override void Enter()
    {
        base.Enter();

        _bowController.SetStartSettings();
    }

    public override void Exit() 
    {
        base.Exit();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _bowController.BeginPull();
        }

        if (Input.GetMouseButton(0))
        {
            _bowController.HoldPull();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ShootProjectile();
            _bowController.ReleasePull();

            DOVirtual.DelayedCall(0.5f, FinishProcess);
        }
    }

    private void ShootProjectile()
    {
        Projectile arrow = Instantiate(_projectile, _bowArrow.transform.position, _bowArrow.transform.rotation);
        arrow.Shoot(_directionCamera.forward, _bowController.PullPower);
    }
}
