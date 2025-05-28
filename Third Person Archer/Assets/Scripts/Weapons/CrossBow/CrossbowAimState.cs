using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using Cinemachine;
using Actor.Properties;

public class CrossbowAimState : ProcessState, IActorIniter
{
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private Transform _crossbowPivot;
    [Space]
    [SerializeField] private Transform _crossbowAimPoint;
    private CrossbowController _crossbowController;
    private AttackInput _attackInput;

    private NormalFov _normalFov;
    private ZoomFovMultiplier _zoomFovMult;
    private WeaponPull _pull;

    private float _lerp;
    private bool _isShooted = false;

    private float _lerpInSpeed = 2f;
    private float _lerpOutSpeed = 8f;

    private float _zoomFov = 45;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out CrossbowController crossbow))
            _crossbowController = crossbow;

        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;

        if (actor.TryGetProperty(out _normalFov)) { }
        if (actor.TryGetProperty(out _zoomFovMult)) { }
        if (actor.TryGetProperty(out _pull)) { }
    }

    public override void Enter()
    {
        base.Enter();

        _crossbowController.ShowView();
        _crossbowController.SetSettings();

        _attackInput.OnAttackRelease.AddListener(Shoot);

        _lerp = 0;
        _isShooted = false;

        _pull.SetValue(_lerp);
    }

    private void Update()
    {
        if (_attackInput.IsHold)
        {
            _lerp += _lerpInSpeed * Time.deltaTime;
            _lerp = Mathf.Clamp(_lerp, 0f, 1f);
        }
        else
        {
            _lerp -= _lerpOutSpeed * Time.deltaTime;
            _lerp = Mathf.Clamp(_lerp, 0f, 1f);
        }

        _pull.SetValue(_lerp);

        _camera.m_Lens.FieldOfView = Mathf.Lerp(_normalFov.Value, _zoomFov * _zoomFovMult.Value, _lerp);

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
        _camera.m_Lens.FieldOfView = _normalFov.Value;

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
