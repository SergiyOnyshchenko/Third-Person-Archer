using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.Properties;

public class OnProjectileOutOfRange : StateTransition, IActorIniter
{
    [SerializeField] private ProjectileRangeConfig _projectileRangeConfig;
    private TraveledDistance _traveledDistance;
    private Range _range;
    private float _outOfRangeDistance;

    public void InitActor(ActorController actor) {
        if (actor.TryGetProperty(out _traveledDistance)) { }
        if (actor.TryGetProperty(out _range)) { }

        _outOfRangeDistance = _projectileRangeConfig.GetDespawnDistance(_range.Value);
    }

    private void Update() {
        if (_traveledDistance.Value > _outOfRangeDistance)
            DoTransition();
    }
}
