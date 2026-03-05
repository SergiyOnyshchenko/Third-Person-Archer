using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor.Properties;

namespace Actor
{
    public class ProjectileMover : System, IActorIniter
    {
        private Transform _transform;
        private Rigidbody _rigidbody;
        private ProjectileStateProperty _projectileState;
        private Speed _speed;

        public ProjectileState State => _projectileState == null ? ProjectileState.Loaded : _projectileState.Value;

        public void InitActor(ActorController actor)
        {
            _transform = actor.transform;
            _rigidbody = actor.GetComponent<Rigidbody>();

            if (actor.TryGetProperty(out _speed)) { }
            if (actor.TryGetProperty(out _projectileState)) { }
        }

        public void Move(Vector3 direction)
        {
            _rigidbody.linearVelocity = direction * _speed.Value;
            _transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }
    }
}