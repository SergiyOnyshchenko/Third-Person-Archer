using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.Properties;
using CustomAnimation;
using DG.Tweening;
using UnityEngine.AI;
using Animator = Actor.Animator;

public class MoveAnimationSubstate : SubState, IActorIniter
{
    private const string SpeedParam = "Speed";
    private const string XDirParam = "XDir";
    private const string ZDirParam = "ZDir";

    [SerializeField] private float _maxSpeed = 7f;
    [Tooltip("Ground speed (m/s) of the clip at each Speed threshold, ascending; thresholds are evenly spaced 0..1. " +
             "Matches the animation to the real velocity (no foot sliding). Empty = linear 0.._maxSpeed.")]
    [SerializeField] private float[] _clipSpeeds;
    [Header("Smoothing")]
    [SerializeField, Min(0f)] private float _speedSmoothTime = 0.12f;
    [SerializeField, Min(0f)] private float _directionSmoothTime = 0.15f;
    [Tooltip("Speed fades to 0 over this time when the state exits, instead of snapping to idle.")]
    [SerializeField, Min(0f)] private float _exitFadeTime = 0.2f;

    private Transform _transform;
    private Animator _animator;
    private Mover _mover;
    private NavMeshAgent _agent;
    private float _currentSpeed;
    private float _speedVelocity;
    private Vector2 _direction;
    private Vector2 _directionVelocity;

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

        // Continue from the current pose (e.g. another move substate or the exit fade), not from idle.
        DOTween.Kill(_animator);
        _currentSpeed = _animator.GetFloat(SpeedParam);
        _direction = new Vector2(_animator.GetFloat(XDirParam), _animator.GetFloat(ZDirParam));
        _speedVelocity = 0f;
        _directionVelocity = Vector2.zero;
    }

    public override void Exit()
    {
        DOTween.Kill(_animator);

        if (_exitFadeTime > 0f && _currentSpeed > 0.001f)
            DOVirtual.Float(_currentSpeed, 0f, _exitFadeTime, value => _animator.SetFloat(SpeedParam, value))
                .SetEase(Ease.OutQuad)
                .SetId(_animator);
        else
            _animator.SetFloat(SpeedParam, 0);

        base.Exit();
    }

    private void Update()
    {
        Vector3 velocity = _mover.Velocity;
        velocity.y = 0f;

        Vector3 moveDirection = velocity.sqrMagnitude > 0.01f || _agent == null
            ? velocity
            : _agent.steeringTarget - _transform.position;

        // A start clip turns the body itself; once it hands over, the body faces the path.
        if (_mover is NavmeshMover navmeshMover && navmeshMover.SuspendTurning)
        {
            _direction = Vector2.up;
            _directionVelocity = Vector2.zero;
        }
        else if (moveDirection.sqrMagnitude > 0.0001f)
        {
            Vector3 relative = _transform.InverseTransformDirection(moveDirection.normalized);
            Vector2 target = new Vector2(relative.x, relative.z);
            _direction = _directionSmoothTime > 0f
                ? Vector2.SmoothDamp(_direction, target, ref _directionVelocity, _directionSmoothTime)
                : target;
        }

        _animator.SetFloat(XDirParam, _direction.x);
        _animator.SetFloat(ZDirParam, _direction.y);

        float speed = SpeedToBlend(velocity.magnitude);

        // While a start / stop clip drives the speed, keep the run blend exactly on it for the hand-over.
        bool clipDriven = _mover is NavmeshMover clipMover && clipMover.SpeedOverride.HasValue;
        _currentSpeed = _speedSmoothTime > 0f && !clipDriven
            ? Mathf.SmoothDamp(_currentSpeed, speed, ref _speedVelocity, _speedSmoothTime)
            : speed;

        _animator.SetFloat(SpeedParam, _currentSpeed);
    }

    private float SpeedToBlend(float speed)
    {
        if (_clipSpeeds == null || _clipSpeeds.Length < 2)
            return Mathf.InverseLerp(0, _maxSpeed, speed);

        int last = _clipSpeeds.Length - 1;
        for (int i = 0; i < last; i++)
            if (speed <= _clipSpeeds[i + 1])
                return (i + Mathf.InverseLerp(_clipSpeeds[i], _clipSpeeds[i + 1], speed)) / last;

        return 1f;
    }
}
