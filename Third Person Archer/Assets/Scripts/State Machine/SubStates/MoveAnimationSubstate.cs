using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Animator = Actor.Animator;

public class MoveAnimationSubstate : SubState, IActorIniter
{
    private Transform _transform;
    private IAnimator _animator;
    private Mover _mover;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;

        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;

        if (actor.TryGetSystem(out Mover mover))
            _mover = mover;
    }

    public override void Enter()
    {
        base.Enter();

        
        
    }

    public override void Exit() 
    { 
        base.Exit();
    }

    private void Update()
    {
        Vector3 moveDirection = _mover.TargetPosition - _transform.position;
        moveDirection = moveDirection.normalized;

        Vector3 relativeDirection = _transform.InverseTransformDirection(moveDirection);

        _animator.SetFloat("XDir", relativeDirection.x);
        _animator.SetFloat("ZDir", relativeDirection.z);

        float speed = Mathf.InverseLerp(0, 6, _mover.Velocity.magnitude);

        _animator.SetFloat("Speed", speed);
    }
}
