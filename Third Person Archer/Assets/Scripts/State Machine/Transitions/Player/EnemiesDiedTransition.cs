using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class EnemiesDiedTransition : StateTransition, IShootingTargetsData
{
    [SerializeField] private bool _allDead;
    [Space]
    [SerializeField] private int _minDeadCount;
    [Space]
    [SerializeField] private float _delayAfter = 1;
    private IEnumerator _checkProcess;
    [SerializeField] private List<Health> _currentTargets = new List<Health>();
    public int TargetDeadCount => _allDead ? _currentTargets.Count : _minDeadCount;

    public void InitShootingTargets(ActorController[] targets)
    {
        foreach (var target in targets)
        {
            if (target.TryGetSystem(out Health health))
                _currentTargets.Add(health);
        }
    }

    public override void Enter()
    {
        base.Enter();

        if (_checkProcess != null)
            StopCoroutine(_checkProcess);

        _checkProcess = CheckProcess();

        StartCoroutine(_checkProcess);
    }

    public override void Exit()
    {
        if (_checkProcess != null)
            StopCoroutine(_checkProcess);

        base.Exit();
    }

    private IEnumerator CheckProcess()
    {
        while (GetDeadCount() < TargetDeadCount)
        {
            Debug.Log("DDDDD " + GetDeadCount() + " " + TargetDeadCount);
            yield return null;
        }

        Debug.Log("DDDDD " + GetDeadCount() + " " + TargetDeadCount);
        yield return new WaitForSeconds(_delayAfter);

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
