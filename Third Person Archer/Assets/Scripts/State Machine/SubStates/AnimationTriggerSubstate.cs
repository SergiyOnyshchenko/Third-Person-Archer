using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class AnimationTriggerSubstate : SubState, IActorIniter
{
    [SerializeField] private string _enterTriggerName;
    [SerializeField] private string _exitTriggerName;
    private IAnimator _animator;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out Actor.Animator animator))
            _animator = animator;
    }

    public override void Enter()
    {
        base.Enter();

        if (!string.IsNullOrEmpty(_enterTriggerName))
        {
            _animator.SetTrigger(_enterTriggerName);
        }
    }

    public override void Exit()
    {
        if (!string.IsNullOrEmpty(_exitTriggerName))
        {
            _animator.SetTrigger(_exitTriggerName);
        }
        
        base.Exit();
    }
}
