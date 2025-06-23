using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;

public class ProjectileTraveledDistanceSubstate : SubState, IActorIniter
{
    private Transform _transform;
    private TraveledDistance _traveledDistance;

    private Vector3 _lastPosition;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;

        if (actor.TryGetProperty(out _traveledDistance)) { }
    }

    private void FixedUpdate()
    {
        Vector3 position = _transform.position;

        if (_lastPosition == Vector3.zero)
        {
            _lastPosition = position;
            return;
        }

        float distance = Vector3.Distance(position, _lastPosition);
        _traveledDistance.SetValue(_traveledDistance.Value + distance);
        _lastPosition = position;
    }
}
