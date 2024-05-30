using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Actor;
using Animator = Actor.Animator;

public abstract class AnimationAttackState : ProcessState, IActorIniter
{
    [SerializeField] private string _attackAnimParamName = "Attack";
    [SerializeField] private string _attackAnimEventName = "Attack";
    [SerializeField] private string _finishAnimEventName = "FinishAttack";
    private Animator _animator;
    private AttackInput _attackInput;
    private AnimationEvent _attackEvent;
    private AnimationEvent _finishEvent;
    public UnityEvent OnAttacked = new UnityEvent();

    public virtual void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;

        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;
    }

    public override void Enter()
    {
        base.Enter();
        PlayShootAnimation();
    }

    public override void Exit()
    {
        if (_attackEvent != null)
            _attackEvent.Event.RemoveListener(Attack);

        if(_finishEvent != null)
            _finishEvent.Event.RemoveListener(Finish);

        base.Exit();
    }

    protected virtual void Attack()
    {
        OnAttacked?.Invoke();
    }

    private void Finish()
    {
        _attackInput.AllowAttack(false);
        FinishProcess();
    }

    private void PlayShootAnimation()
    {
        _animator.SetTrigger(_attackAnimParamName);

        if (_animator.TryGetAnimationEvent(_attackAnimEventName, out AnimationEvent shootEvent))
        {
            _attackEvent = shootEvent;
            _attackEvent.Event.AddListener(Attack);
        }

        if (_animator.TryGetAnimationEvent(_finishAnimEventName, out AnimationEvent finishEvent))
        {
            _finishEvent = finishEvent;
            _finishEvent.Event.AddListener(Finish);
        }
    }
}
