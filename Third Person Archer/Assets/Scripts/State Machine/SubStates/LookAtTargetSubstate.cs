using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class LookAtTargetSubstate : SubState, IActorIniter
{
    [SerializeField] private float _rotationSpeed = 10;
    private PerceptionInput _input;
    private Transform _transform;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;

        if (actor.TryGetInput(out PerceptionInput input))
            _input = input;
    }

    private void Update()
    {
        Vector3 myPosition = _transform.position;
        myPosition.y = 0;

        Vector3 targetPosition = _input.Target.transform.position;
        targetPosition.y = 0;

        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - myPosition);
        _transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, Time.deltaTime * _rotationSpeed);
    }
}
