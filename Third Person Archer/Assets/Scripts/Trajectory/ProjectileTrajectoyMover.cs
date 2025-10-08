using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;

namespace Actor
{
    public class ProjectileTrajectoyMover : System, IActorIniter
    {
        [SerializeField] private bool _faceVelocity = true;
        [SerializeField] private float _minSpeedForFacing = 0.01f;
        private Rigidbody _rigidbody;
        private Transform _transform;
        private ProjectileDirection _direction;
        private Gravity _gravity;

        public void InitActor(ActorController actor)
        {
            _rigidbody = actor.GetComponent<Rigidbody>();
            _transform = actor.transform;

            if (actor.TryGetProperty(out _gravity)) { }
            if (actor.TryGetProperty(out _direction)) { }
        }

        public void Launch()
        {
            _rigidbody.useGravity = false;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.velocity = _direction.Value;
        }
        
        public void Move()
        {
            if (_rigidbody == null)
                return;

            Vector3 g = new Vector3(0, -_gravity.Value, 0);
            _rigidbody.AddForce(g * _rigidbody.mass, ForceMode.Acceleration);

            if (_faceVelocity)
            {
                Vector3 v = _rigidbody.velocity;
                if (v.sqrMagnitude > (_minSpeedForFacing * _minSpeedForFacing))
                {
                    _transform.rotation = Quaternion.LookRotation(v.normalized, Vector3.up);
                }
            }
        }
    }
}