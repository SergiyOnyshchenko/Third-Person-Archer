using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraChangeSubstate : SubState
{
    [SerializeField] private CinemachineVirtualCamera _camera;
    private float _fovMultiplier = 0.5f;

    public override void Enter()
    {
        base.Enter();
        _camera.Priority = 2;
        _camera.m_Lens.FieldOfView = _camera.m_Lens.FieldOfView * _fovMultiplier;
    }

    public override void Exit()
    {
        _camera.Priority = 0;
        base.Exit();
    }
}
