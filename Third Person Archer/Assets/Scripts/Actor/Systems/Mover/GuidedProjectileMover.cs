using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;

namespace Actor
{
    public class GuidedProjectileMover : System, IActorIniter
    {
        private float rotationSpeed = 750f;
        float rotationAcceleration = 5f;

        private Vector2 _rotation = Vector2.zero;

        private float currentYawSpeed = 0f;
        private float currentPitchSpeed = 0f;

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

        public void Move(float horizontal, float vertical)
        {
            float targetYawSpeed = horizontal * rotationSpeed;
            float targetPitchSpeed = -vertical * rotationSpeed;

            currentYawSpeed = Mathf.Lerp(currentYawSpeed, targetYawSpeed, rotationAcceleration * Time.fixedDeltaTime);
            currentPitchSpeed = Mathf.Lerp(currentPitchSpeed, targetPitchSpeed, rotationAcceleration * Time.fixedDeltaTime);

            float yawRotation = currentYawSpeed * Time.fixedDeltaTime;
            float pitchRotation = currentPitchSpeed * Time.fixedDeltaTime;

            _transform.Rotate(pitchRotation, yawRotation, 0f, Space.Self);
            _rigidbody.velocity = _transform.forward * _speed.Value * Time.fixedDeltaTime;
        }
    }
}