using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Actor.Properties;

namespace Actor
{
    public class BodyRotator : System, IActorIniter
    {
        private RotateSpeed _speed;
        private RotateDuration _duration;
        private RotateEase _ease;
        private Transform _transform;

        public void InitActor(ActorController actor)
        {
            _transform = actor.transform;

            if(actor.TryGetProperty(out RotateSpeed speed))
                _speed = speed;

            if (actor.TryGetProperty(out RotateDuration duration))
                _duration = duration;

            if (actor.TryGetProperty(out RotateEase ease))
                _ease = ease;
        }

        public void RotateToInstant(Transform lookTarget)
        {
            Vector3 myPosition = _transform.position;
            myPosition.y = 0;

            Vector3 targetPosition = lookTarget.transform.position;
            targetPosition.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - myPosition);
            RotateToInstant(targetRotation);
        }

        public void RotateToInstant(Quaternion targetRotation)
        {
            _transform.DORotateQuaternion(targetRotation, _duration.Value).SetEase(_ease.Value);
        }
    }
}
