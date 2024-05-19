using System;
using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class PlayerHitPointsShootingState : MainState
{
    [SerializeField] ActorController _target;
    [SerializeField] private int[] _hitPointsIndexes;

    public override void Enter()
    {
        base.Enter();


    }

    private HitPoint[] GetHitPoints()
    {
        if(_target.TryGetSystem(out HitPointManager hitPointManager))
        {
            List<HitPoint> hitPoints = new List<HitPoint>();

            for (int i = 0; i < _hitPointsIndexes.Length; i++)
            {
                if (hitPointManager.TryGetHitPoint(_hitPointsIndexes[i], out HitPoint point))
                {
                    hitPoints.Add(point);
                }
            }

            return hitPoints.ToArray();
        }
        else
        {
            return null;
        }
    }
}
