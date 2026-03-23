using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using UnityEngine.Windows;
using DG.Tweening;
using UnityEngine.AI;

public class JumpState : ProcessState, IActorIniter
{
    [SerializeField] private AnimationCurve _jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    private Transform _target;
    private JumpInput _input;
    private BodyRotator _rotator;

    public void InitActor(ActorController actor)
    {
        _target = actor.transform;

        if (actor.TryGetInput(out JumpInput input))
            _input = input;

        if (actor.TryGetSystem(out BodyRotator rotator))
            _rotator = rotator;
    }

    public override void Enter()
    {
        base.Enter();

        Jump(_input.JumpSpline);
    }

    public void Jump(Spline jumpSpline)
    {
        float elapsed = 0f;
        float duration = 0.75f;

        var endPosition = jumpSpline.CalculatePosition(1f);
        _rotator.RotateToInstant(endPosition);

        DOTween.To(() => elapsed, x => elapsed = x, 1f, duration)
            .OnUpdate(() =>
            {
                float curvedValue = _jumpCurve.Evaluate(elapsed);
                _target.transform.position = jumpSpline.CalculatePosition(curvedValue);
            })
            .SetEase(Ease.Linear)
            .OnComplete(FinishProcess);
    }
}
