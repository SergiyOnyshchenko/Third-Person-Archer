using System.Collections;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;
using Input = UnityEngine.Input;

public class SpearThrowState : ProcessState, IActorIniter
{
    [SerializeField] private Projectile _projectile;
    [SerializeField] private Transform _directionCamera;
    [SerializeField] private GameObject _spearModel;
    private SpearController _spearController;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out SpearController spear))
            _spearController = spear;
    }

    public override void Enter()
    {
        base.Enter();

        _spearController.SetStartSettings();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _spearController.BeginPull();
        }

        if (Input.GetMouseButton(0))
        {
            _spearController.HoldPull();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ShootProjectile();
            _spearController.ReleasePull();

            DOVirtual.DelayedCall(0.5f, FinishProcess);
        }
    }

    private void ShootProjectile()
    {
        Projectile arrow = Instantiate(_projectile, _spearModel.transform.position, _spearModel.transform.rotation);
        arrow.Shoot(_directionCamera.forward, _spearController.PullPower, null);
    }
}
