using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class AimViewIK : MonoBehaviour, IActorIniter
{
    [SerializeField] private Transform _aimIK;
    [SerializeField] private Transform _eyesPoint;
    private AimInput _aimer;
    private Vector3 _normalPosition;

    private void Awake()
    {
        _normalPosition = _aimIK.transform.localPosition;
    }

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetInput(out AimInput aimer))
            _aimer = aimer;
    }

    private void LateUpdate()
    {
        if (_aimer.GetAimTarget() == null)
        {
            _aimIK.transform.localPosition = _normalPosition;
        }
        else
        {
            _aimIK.transform.position = _aimer.GetAimTarget().position;
        }
    }
}
