using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using Cinemachine;
using UnityEngine;

public class PullCameraMotion : SubState, IActorIniter
{
    [SerializeField] private float _zoomFov = 30;
    [Space]
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private Transform _pullHolder;
    private NormalFov _normalFov;
    private ZoomFovMultiplier _zoomFovMult;

    private const float _springPower = 8f;
    private const float _springDumping = 0.5f;

    private SpringFloat _spring;
    private IPull _pull;


    public void InitActor(ActorController actor)
    {
        if(actor.TryGetProperty(out _normalFov)) 
        {
            _pull = _pullHolder.GetComponent<IPull>();
            _spring = new SpringFloat(_springPower, _springDumping, _normalFov.Value);
        }

        if (actor.TryGetProperty(out _zoomFovMult)) { }
    }

    private void FixedUpdate()
    {
        UpdateCameraZoom(_pull.PullPower);
    }

    private void UpdateCameraZoom(float value)
    {
        float fov = Mathf.Lerp(_normalFov.Value, _zoomFov * _zoomFovMult.Value, value);
        _spring.UpdateValue(fov);
        _camera.m_Lens.FieldOfView = _spring.Value;
    }

    public void Reset()
    {
        _camera.m_Lens.FieldOfView = _normalFov.Value;
    }
}
