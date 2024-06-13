using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using DG.Tweening;

public class AnimationTriggerSubstate : SubState, IActorIniter
{
    [SerializeField] private string _enterTriggerName;
    [SerializeField] private float _enterDelay;
    [Space]
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
            DOVirtual.DelayedCall(_enterDelay, () => _animator.SetTrigger(_enterTriggerName));
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
