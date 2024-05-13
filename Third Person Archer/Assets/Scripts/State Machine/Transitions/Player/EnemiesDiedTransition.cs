using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class EnemiesDiedTransition : StateTransition, IShootingTargetsData
{
    [SerializeField] private bool _allDead;
    [Space]
    [SerializeField] private int _minDeadCount;
    private List<Health> _currentTargets = new List<Health>();
    public int TargetDeadCount => _allDead ? _currentTargets.Count : _minDeadCount;

    public void InitShootingTargets(ActorController[] targets)
    {
        foreach (var target in targets)
        {
            if (target.TryGetSystem(out Health health))
                _currentTargets.Add(health);
        }
    }

    private void Update()
    {
        if (GetDeadCount() >= TargetDeadCount)
            DoTransition();
    }

    private int GetDeadCount()
    {
        int count = 0;

        foreach (var target in _currentTargets)
        {
            if (target.IsDead)
                count ++;
        }

        return count;
    }
}
