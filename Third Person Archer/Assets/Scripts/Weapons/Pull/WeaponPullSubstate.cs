using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class WeaponPullSubstate : SubState, IActorIniter
{
    [Tooltip("Multiplier on the weapon balance draw speed (heavy weapons < 1, quick ones > 1).")]
    [SerializeField] private float _pullSpeed = 1f;
    [SerializeField] private AnimationCurve _pullCurve = AnimationCurve.Linear(0, 0, 1, 1);
    private AttackInput _attackInput;
    private WeaponPull _pull;
    private WeaponBalance _balance;

    private bool _isPulling;
    private bool _wasHolding;
    private float _rawPull;
    private float _written;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetInput(out _attackInput)) { }
        if (actor.TryGetProperty(out _pull)) { }
        if (actor.TryGetProperty(out _balance)) { }
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

        // The weapon zeroed the pull itself (threw / shot): don't relax a draw that's already gone.
        if (!isHolding && _rawPull > 0f && _pull.Value < _written - 0.0001f)
            _rawPull = 0f;

        if (isHolding && !_wasHolding)
        {
            BeginPull();
        }
        else if (!isHolding && _wasHolding)
        {
            ReleasePull();
        }

        if (isHolding && _isPulling)
        {
            HoldPull(_balance.Speed * _pullSpeed);
        }
        else if (_rawPull > 0f)
        {
            HoldPull(-_balance.Speed * _pullSpeed);
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

    private void HoldPull(float speed)
    {
        // Real time: the slow-mo while drawing slows the world, not the player's hands.
        _rawPull += TimeScaleUtils.AimDeltaTime * speed;
        _rawPull = Mathf.Clamp01(_rawPull);

        _written = Mathf.Clamp01(_pullCurve.Evaluate(_rawPull));
        _pull.SetPullValue(_written);
    }
}