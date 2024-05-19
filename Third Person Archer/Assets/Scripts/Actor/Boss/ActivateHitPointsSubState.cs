using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using static RootMotion.FinalIK.HitReaction;

public class ActivateHitPointsSubState : SubState, IActorIniter
{
    [SerializeField] private int[] _hitPointsIndexes;
    [SerializeField] private HitPoint[] _hitPoints;
    private HitPointManager _hitPointManager;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out HitPointManager hitPointManager))
            _hitPointManager = hitPointManager;

        InitHitPoints();
    }

    public override void Enter()
    {
       

        base.Enter();

        foreach (var point in _hitPoints)
            point.Activate();
    }

    public override void Exit() 
    {
        foreach (var point in _hitPoints)
            point.Desactivate();

        base.Exit();
    }

    private void InitHitPoints()
    {
        List<HitPoint> hitPoints = new List<HitPoint>();

        for (int i = 0; i < _hitPointsIndexes.Length; i++)
            if (_hitPointManager.TryGetHitPoint(_hitPointsIndexes[i], out HitPoint point))
                hitPoints.Add(point);

        _hitPoints = hitPoints.ToArray();

        IHitPointsData[] data = GetComponentsInChildren<IHitPointsData>();

        foreach (var item in data)
            item.InitHitPoints(_hitPoints);
    }
}
