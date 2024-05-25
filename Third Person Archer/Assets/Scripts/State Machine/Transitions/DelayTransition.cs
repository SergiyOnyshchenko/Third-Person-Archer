using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelayTransition : StateTransition
{
    [SerializeField] private float _delay;
    private float _timer;
    public float Ratio { get => _timer / _delay; }

    public override void Enter()
    {
        base.Enter();

        _timer = _delay;
    }

    private void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0)
            DoTransition();
    }
}
