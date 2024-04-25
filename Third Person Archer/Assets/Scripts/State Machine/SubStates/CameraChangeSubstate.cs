using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraChangeSubstate : SubState
{
    [SerializeField] private CinemachineVirtualCamera _camera;

    public override void Enter()
    {
        base.Enter();
        _camera.Priority = 2;
    }

    public override void Exit()
    {
        _camera.Priority = 0;
        base.Exit();
    }
}
