using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;
using Actor;

public class BowShootingCameraSubstate : SubState, IActorIniter
{
    [SerializeField] private CinemachineVirtualCamera _camera;
    private const float _springPower = 8f;
    private const float _springDumping = 0.5f;
    private SpringFloat _spring;
    private BowController _bowController;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out BowController bow))
            _bowController = bow;
    }

    private void Start()
    {
        _spring = new SpringFloat(_springPower, _springDumping, 90);
    }

    public override void Enter()
    {
        base.Enter();
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void FixedUpdate()
    {
        UpdateCameraZoom(_bowController.PullPower);
    }

    private void UpdateCameraZoom(float value)
    {
        float fov = Mathf.Lerp(90, 60, value);
        _spring.UpdateValue(fov);
        _camera.m_Lens.FieldOfView = _spring.Value;
    }
}
