using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using Cinemachine;

public class CrossbowAimState : ProcessState, IActorIniter
{
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private Transform _crossbowPivot;
    [Space]
    [SerializeField] private Transform _crossbowAimPoint;
    private CrossbowController _crossbowController;
    private AttackInput _attackInput;
    private float _lerp;
    private bool _isShooted = false;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out CrossbowController crossbow))
            _crossbowController = crossbow;

        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;
    }

    public override void Enter()
    {
        base.Enter();

        _crossbowController.ShowView();
        _crossbowController.SetSettings();

        _attackInput.OnAttackRelease.AddListener(Shoot);

        _lerp = 0;
        _isShooted = false;
    }

    private void Update()
    {
        if (_attackInput.IsHold)
        {
            _lerp += 2 * Time.deltaTime;
            _lerp = Mathf.Clamp(_lerp, 0f, 1f);
        }
        else
        {
            _lerp -= 8 * Time.deltaTime;
            _lerp = Mathf.Clamp(_lerp, 0f, 1f);
        }

        _camera.m_Lens.FieldOfView = Mathf.Lerp(90, 30, _lerp);

        _crossbowPivot.localPosition = Vector3.Lerp(Vector3.zero, _crossbowAimPoint.localPosition, _lerp);
        _crossbowPivot.localRotation = Quaternion.Lerp(Quaternion.identity, _crossbowAimPoint.localRotation, _lerp);

        if (_isShooted && _lerp <= 0)
        {
            FinishProcess();
        }
    }

    private void LateUpdate()
    {
        _crossbowController.UpdateHands();
    }

    public override void Exit()
    {
        _camera.m_Lens.FieldOfView = 90;

        _attackInput.OnAttackRelease.RemoveListener(Shoot);

        base.Exit();
    }
    private void Shoot()
    {
        if (_lerp >= 1)
        {
            _attackInput.OnAttackRelease.RemoveListener(Shoot);
            _crossbowController.Shoot(null);
            _isShooted = true;
        }
    }
}
