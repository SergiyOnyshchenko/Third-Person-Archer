using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.Properties;

public class ProjectileDirectMoveSubstate : SubState, IActorIniter
{
    private Transform _transform;
    private Rigidbody _rigidbody;
    private ProjectileStateProperty _projectileState;
    private Speed _speed;
    private ProjectileDirection _direction;
    public ProjectileState State => _projectileState == null ? ProjectileState.Loaded : _projectileState.Value;

    public void InitActor(ActorController actor)
    {
        _transform = actor.transform;
        _rigidbody = actor.GetComponent<Rigidbody>();

        if (actor.TryGetProperty(out _speed)) { }
        if (actor.TryGetProperty(out _direction)) { }
        if (actor.TryGetProperty(out _projectileState)) { }
    }

    public void FixedUpdate()
    {
        Vector3 direction = _transform.forward;

        if (_direction != null)
            direction = _direction.Value;

        Move(direction);
    }

    private void Move(Vector3 direction)
    {
        if(_rigidbody != null)
            _rigidbody.linearVelocity = direction * _speed.Value;

        _transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }
}
