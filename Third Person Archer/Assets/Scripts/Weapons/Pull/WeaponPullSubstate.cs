using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class WeaponPullSubstate : SubState, IActorIniter
{
    [SerializeField] private float _pullSpeed = 1f;
    [SerializeField] private AnimationCurve _pullCurve = AnimationCurve.Linear(0, 0, 1, 1);
    private AttackInput _attackInput;
    private WeaponPull _pull;

    private bool _isPulling;
    private bool _wasHolding;
    private float _rawPull;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out _attackInput)) { }
        if (actor.TryGetProperty(out _pull)) { }
    }

    public override void Enter()
    {
        base.Enter();

        _isPulling = false;
        _wasHolding = false;
        _rawPull = 0f;
        _pull.SetPullValue(0f);
    }

    public override void Exit()
    {
        base.Exit();
        _isPulling = false;
        _wasHolding = false;
        _rawPull = 0f;
    }

    private void Update()
    {
        if (_attackInput == null || _pull == null)
            return;

        bool isHolding = _attackInput.IsHold;

        // Detect edge: begin pull
        if (isHolding && !_wasHolding)
        {
            BeginPull();
        }
        // Detect edge: release pull
        else if (!isHolding && _wasHolding)
        {
            ReleasePull();
        }

        // Continuous pulling
        if (isHolding)
        {
            HoldPull();
        }

        _wasHolding = isHolding;
    }

    private void BeginPull()
    {
        _isPulling = true;
        _rawPull = 0f;
        _pull.BeginPull();
    }

    private void ReleasePull()
    {
        _isPulling = false;
        _pull.ReleasePull();
    }

    private void HoldPull()
    {
        if (!_isPulling)
            return;

        _rawPull += Time.deltaTime * _pullSpeed;
        _rawPull = Mathf.Clamp01(_rawPull);

        float curvedValue = _pullCurve.Evaluate(_rawPull);
        _pull.SetPullValue(curvedValue);
    }
}