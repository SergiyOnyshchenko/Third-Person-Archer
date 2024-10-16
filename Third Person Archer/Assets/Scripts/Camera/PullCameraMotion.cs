using System.Collections;
using System.Collections.Generic;
using Actor;
using Cinemachine;
using UnityEngine;

public class PullCameraMotion : SubState
{
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private Transform _pullHolder;
    private const float _springPower = 8f;
    private const float _springDumping = 0.5f;
    private float _normalFov = 90;
    private float _zoomFov = 60;
    private SpringFloat _spring;
    private IPull _pull;

    private void Start()
    {
        _pull = _pullHolder.GetComponent<IPull>();
        _spring = new SpringFloat(_springPower, _springDumping, 90);
    }

    private void FixedUpdate()
    {
        UpdateCameraZoom(_pull.PullPower);
    }

    private void UpdateCameraZoom(float value)
    {
        float fov = Mathf.Lerp(_normalFov, _zoomFov, value);
        _spring.UpdateValue(fov);
        _camera.m_Lens.FieldOfView = _spring.Value;
    }

    public void Reset()
    {
        _camera.m_Lens.FieldOfView = _normalFov;
    }
}
