using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomDelayTransition : StateTransition
{
    [SerializeField] private float _minDelay;
    [SerializeField] private float _maxDelay;
    private float _timer;

    public override void Enter()
    {
        base.Enter();

        _timer = Random.Range(_minDelay, _maxDelay);
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0)
            DoTransition();
    }
}
