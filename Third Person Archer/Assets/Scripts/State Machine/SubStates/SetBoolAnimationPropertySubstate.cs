using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Animator = Actor.Animator;

public class SetBoolAnimationPropertySubstate : SubState, IActorIniter
{
    [SerializeField] private string _propertyName;
    [Header("Enter")]
    [SerializeField] private bool _invokeOnEnter = true;
    [SerializeField] private bool _valueOnEnter;
    [Header("Exit")]
    [SerializeField] private bool _invokeOnExit = true;
    [SerializeField] private bool _valueOnExit;
    private IAnimator _animator;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;
    }

    public override void Enter()
    {
        base.Enter();

        if (_invokeOnEnter)
            _animator.SetBool(_propertyName, _valueOnEnter);
    }

    public override void Exit()
    {
        if (_invokeOnExit)
            _animator.SetBool(_propertyName, _valueOnExit);

        base.Exit();
    }
}



