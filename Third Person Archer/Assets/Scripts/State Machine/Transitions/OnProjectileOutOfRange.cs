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

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out _traveledDistance)) { }
        if (actor.TryGetProperty(out _range)) { }
    }

    private void Start()
    {
        if (_range != null)
            _outOfRangeDistance = _projectileRangeConfig.GetDespawnDistance();
        else
            _outOfRangeDistance = 100;
    }

    private void Update()
    {

        if (_traveledDistance.Value > _outOfRangeDistance)
        {
            //Debug.Log("Projectile Distance " + _traveledDistance.Value + " | Out Of Range Distance " + _outOfRangeDistance);
            DoTransition();
        }

    }
}
