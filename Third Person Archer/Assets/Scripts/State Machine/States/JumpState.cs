using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using UnityEngine.Windows;
using DG.Tweening;
using UnityEngine.AI;

public class JumpState : ProcessState, IActorIniter
{
    private Transform _target;
    private JumpInput _input;

    public void InitActor(ActorController actor)
    {
        _target = actor.transform;

        if (actor.TryGetInput(out JumpInput input))
            _input = input;
    }

    public override void Enter()
    {
        base.Enter();

        Jump(_input.JumpSpline);
    }

    public void Jump(Spline jumpSpline)
    {
        float value = 0f;
        float duration = 0.75f;

        DOTween.To(() => value, x => value = x, 0.5f, duration/2)
        .OnUpdate(() => 
        {
            _target.transform.position = jumpSpline.CalculatePosition(value);
        }).
        SetEase(Ease.OutSine);

        DOTween.To(() => value, x => value = x, 1f, duration/2)
        .OnUpdate(() =>
        {
            _target.transform.position = jumpSpline.CalculatePosition(value);
        }).
        SetEase(Ease.InSine).
        SetDelay(duration / 2).
        OnComplete(FinishProcess);
    }
}
