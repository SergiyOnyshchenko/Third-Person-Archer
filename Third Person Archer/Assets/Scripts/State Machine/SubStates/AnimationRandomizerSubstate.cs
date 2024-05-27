using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using Animator = Actor.Animator;

public class AnimationRandomizerSubstate : SubState, IActorIniter
{
    [SerializeField] private string _animationParamName = "Type";
    [Space]
    [SerializeField] private int _minValue;
    [SerializeField] private int _maxValue;
    private IAnimator _animator;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;
    }

    public override void Enter()
    {
        base.Enter();

        int randomizedValue = Random.Range(_minValue, _maxValue);
        _animator.SetInteger(_animationParamName, randomizedValue);
    }

    public override void Exit() 
    { 
        base.Exit();
    }
}
