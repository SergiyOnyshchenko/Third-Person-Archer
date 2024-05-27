using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using System;

public class LookAtTargetSubstate : SubState, IActorIniter
{
    public enum TargetType
    {
        Manual,
        Perception
    }

    [SerializeField] private TargetType _targetType;
    [SerializeField] private float _rotationSpeed = 10;
    [SerializeField] private bool _useY;
    [Space]
    [SerializeField] private Transform _manualTarget;
    private PerceptionInput _input;
    private Transform _transform;
    private Transform _target;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;

        if (actor.TryGetInput(out PerceptionInput input))
            _input = input;
    }

    public override void Enter()
    {
        base.Enter();

        switch (_targetType)
        {
            case TargetType.Manual:
                _target = _manualTarget;
                break;
            case TargetType.Perception:
                _target = _input.Target.TargetPoint;
                break;
        }
    }

    private void Update()
    {
        Vector3 myPosition = _transform.position;

        if(!_useY)
            myPosition.y = 0;

        Vector3 targetPosition = _target.position;

        if (!_useY)
            targetPosition.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - myPosition);
        _transform.rotation = Quaternion.RotateTowards(_transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }
}
