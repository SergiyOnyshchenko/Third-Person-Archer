using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ActivateRandomHitPointsSubState : SubState, IActorIniter
{
    [SerializeField] private List<HitPoint> _hitPoints = new List<HitPoint>();
    private HitPointManager _hitPointManager;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out HitPointManager hitPointManager))
            _hitPointManager = hitPointManager;
    }

    public override void Enter()
    {
        base.Enter();

        _hitPoints.Clear();

        while (_hitPoints.Count < 3)
        {
            HitPoint hitPoint = _hitPointManager.GetRandomHitPoint();

            if (_hitPoints.Contains(hitPoint))
                continue;

            _hitPoints.Add(hitPoint);
        }

        foreach (var point in _hitPoints)
            point.Activate();
    }

    public override void Exit()
    {
        foreach (var point in _hitPoints)
            point.Desactivate();

        base.Exit();
    }
}
