using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllHitPointsHitedTransition : StateTransition, IHitPointsData
{
    [SerializeField] private HitPoint[] _hitPoints;
    private int _hitedCount;

    public void InitHitPoints(HitPoint[] hitPoints)
    {
        _hitPoints = hitPoints;
    }

    public override void Enter()
    {
        base.Enter();

        _hitedCount = 0;

        foreach (var hitPoint in _hitPoints)
        {
            hitPoint.OnDamaged.AddListener(CheckHited);
        }
    }

    public override void Exit() 
    {
        foreach (var hitPoint in _hitPoints)
            hitPoint.OnDamaged.RemoveListener(CheckHited);

        base.Exit();
    }

    private void CheckHited()
    {
        _hitedCount++;

        Debug.Log("HITED COUNT - " + _hitedCount);

        if (_hitedCount < _hitPoints.Length)
            return;

        DoTransition();
    }
}
