using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using Cinemachine;
using UnityEngine;

public class PullCameraMotion : SubState, IActorIniter
{
    [SerializeField] private CinemachineVirtualCamera _camera;
    [SerializeField] private Transform _pullHolder;
    private NormalFov _normalFov;
    private ZoomMagnification _zoomMagnification;
    private ZoomFovMultiplier _zoomFovMult;

    private const float _springPower = 8f;
    private const float _springDumping = 0.5f;

    private SpringFloat _spring;
    private IPull _pull;
    
    public float NormalFov => (_normalFov != null && _normalFov.Value != 0) ? _normalFov.Value : 60;
    public float ZoomMagnification => (_zoomMagnification != null && _zoomMagnification.Value != 0) ? _zoomMagnification.Value : 1;
    public float ZoomFovMult => (_zoomFovMult != null && _zoomFovMult.Value != 0) ? _zoomFovMult.Value : 1;


    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out _normalFov))
        {
            _pull = _pullHolder.GetComponent<IPull>();
            _spring = new SpringFloat(_springPower, _springDumping, _normalFov.Value);
        }

        if (actor.TryGetProperty(out _zoomMagnification)) { }
        if (actor.TryGetProperty(out _zoomFovMult)) { }
    }

    public override void Exit()
    {
        base.Exit();
    }

    private void FixedUpdate()
    {
        UpdateCameraZoom(_pull.PullPower);
    }

    private void UpdateCameraZoom(float value)
    {
        float totalMag = Mathf.Max(1f, ZoomMagnification * ZoomFovMult);
        float aimFov  = Mathf.Clamp(FovUtils.FovFromMagnification(NormalFov, totalMag), 4, 90);
        float targetFov = Mathf.Lerp(NormalFov, aimFov, Mathf.Clamp01(value));

        _spring.UpdateValue(targetFov);
        _camera.m_Lens.FieldOfView = _spring.Value;
    }

    public void Reset()
    {
        _camera.m_Lens.FieldOfView = _normalFov.Value;
    }
}
