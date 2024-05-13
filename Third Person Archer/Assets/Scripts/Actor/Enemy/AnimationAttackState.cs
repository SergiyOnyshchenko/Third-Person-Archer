using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Actor;
using Animator = Actor.Animator;

public abstract class AnimationAttackState : MainState, IActorIniter
{
    [SerializeField] private string _attackAnimParamName = "Attack";
    [SerializeField] private string _attackAnimEventName = "Attack";
    private Animator _animator;
    private AnimationEvent _attackEvent;
    public UnityEvent OnAttacked = new UnityEvent();

    public virtual void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;
    }

    public override void Enter()
    {
        base.Enter();
        PlayShootAnimation();
    }

    protected virtual void Attack()
    {
        _attackEvent.Event.RemoveListener(Attack);
        OnAttacked?.Invoke();
    }

    private void PlayShootAnimation()
    {
        _animator.SetTrigger(_attackAnimParamName);

        if (_animator.TryGetAnimationEvent(_attackAnimEventName, out AnimationEvent shootEvent))
        {
            _attackEvent = shootEvent;
            _attackEvent.Event.AddListener(Attack);
        }
    }
}
