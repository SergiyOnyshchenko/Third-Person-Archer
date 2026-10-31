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
    [Tooltip("Seconds the enemy holds its aim before the attack animation starts, so a quick attack can still be read and stopped.")]
    [SerializeField, Min(0f)] private float _windup;
    private ActorController _actor;
    private Animator _animator;
    private AttackInput _attackInput;
    private AnimationEvent _attackEvent;
    private AnimationEvent _finishEvent;
    private Coroutine _windupRoutine;
    public UnityEvent OnAttacked = new UnityEvent();

    public virtual void InitActor(ActorController actor)
    {
        _actor = actor;

        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;

        if (actor.TryGetInput(out AttackInput attackInput))
            _attackInput = attackInput;
    }

    public override void Enter()
    {
        base.Enter();

        // The AttackDirector slot this attack was granted stays taken until the attack ends.
        AttackDirector.NotifyStarted(_actor);

        if (_windup > 0f)
            _windupRoutine = StartCoroutine(PlayAfterWindup());
        else
            PlayShootAnimation();
    }

    public override void Exit()
    {
        if (_windupRoutine != null)
        {
            StopCoroutine(_windupRoutine);
            _windupRoutine = null;
        }

        if (_attackEvent != null)
            _attackEvent.Event.RemoveListener(Attack);

        if(_finishEvent != null)
            _finishEvent.Event.RemoveListener(Finish);

        AttackDirector.Release(_actor);

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

    private IEnumerator PlayAfterWindup()
    {
        yield return new WaitForSeconds(_windup);
        _windupRoutine = null;
        PlayShootAnimation();
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
