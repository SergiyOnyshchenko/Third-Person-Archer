using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class BowShootingCameraSubstate : SubState
{
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] protected BowShootingState _shooting;
    private const float _springPower = 2.5f;
    private const float _springDumping = 0.5f;
    private SpringFloat _spring;

    private void Start()
    {
        _spring = new SpringFloat(_springPower, _springDumping, 90);
    }

    public override void Enter()
    {
        base.Enter();
    }

    private void Update()
    {
        float fov = Mathf.Lerp(90, 110, _shooting.PullPower);

        _spring.UpdateValue(fov);

        _camera.m_Lens.FieldOfView = _spring.Value;
    }
}
