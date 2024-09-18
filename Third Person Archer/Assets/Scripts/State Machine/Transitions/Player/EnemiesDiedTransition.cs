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
    [SerializeField] private List<HostageController> _currentHostages = new List<HostageController>();
    public int TargetDeadCount => _allDead ? _currentTargets.Count : _minDeadCount;

    public void InitShootingTargets(ActorController[] targets, ActorController[] hostages)
    {
        foreach (var target in targets)
        {
            if (target.TryGetSystem(out Health health))
                _currentTargets.Add(health);
        }

        foreach (var hostage in hostages)
        {
            if(hostage.TryGetSystem(out HostageController controller))
                //if(controller.IsJailed)
                    _currentHostages.Add(controller);
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
        yield return new WaitForSeconds(0.5f);

        while (GetDeadCount() < TargetDeadCount)
        {
            yield return null;
        }
        
        while (!AllHostagesSavedOrDead())
        {
            yield return null;
        }

        foreach (var hostage in _currentHostages)
        {
            hostage.Save();
        }

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

    private bool AllHostagesSavedOrDead()
    {
        foreach (var hostage in _currentHostages)
        {
            if (hostage.IsJailed && !hostage.IsDead)
                return false;
        }

        return true;
    }
}
