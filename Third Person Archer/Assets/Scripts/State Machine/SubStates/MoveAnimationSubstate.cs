using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.Properties;
using CustomAnimation;
using UnityEngine.AI;
using Animator = Actor.Animator;

public class MoveAnimationSubstate : SubState, IActorIniter
{
    [SerializeField] private float _maxSpeed = 7f;
    private Transform _transform;
    private IAnimator _animator;
    private Mover _mover;
    private NavMeshAgent _agent;
    private float _currentSpeed;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;

        if (actor.TryGetSystem(out Animator animator))
            _animator = animator;

        if (actor.TryGetSystem(out Mover mover))
            _mover = mover;

        _agent = actor.GetComponent<NavMeshAgent>();
    }

    public override void Enter()
    {
        base.Enter();
        _currentSpeed = 0;
    }

    public override void Exit() 
    {
        _animator.SetFloat("Speed", 0);

        base.Exit();
    }

    private void Update()
    {
        Vector3 moveDirection = _agent.steeringTarget - _transform.position;
        moveDirection = moveDirection.normalized;

        Vector3 relativeDirection = _transform.InverseTransformDirection(moveDirection);

        _animator.SetFloat("XDir", relativeDirection.x);
        _animator.SetFloat("ZDir", relativeDirection.z);

        float speed = Mathf.InverseLerp(0, _maxSpeed, _mover.Velocity.magnitude);

        //_currentSpeed = Mathf.MoveTowards(_currentSpeed, speed, 1f * Time.deltaTime);

        _animator.SetFloat("Speed", speed);
    }
}
