using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Actor.Properties;

namespace Actor
{
    public class BodyRotator : System, IActorIniter
    {
        [SerializeField] private Transform _transformY;

        private RotateSpeed _speed;
        private RotateDuration _duration;
        private RotateEase _ease;
        private Transform _transform;

        public void InitActor(ActorController actor)
        {
            _transform = actor.transform;

            if (actor.TryGetProperty(out RotateSpeed speed))
                _speed = speed;

            if (actor.TryGetProperty(out RotateDuration duration))
                _duration = duration;

            if (actor.TryGetProperty(out RotateEase ease))
                _ease = ease;
        }

        public void RotateToInstant(Transform lookTarget)
        {
            RotateToInstant(lookTarget.position);
        }

        public void RotateToInstant(Vector3 lookTarget)
        {
            Vector3 myPosition = _transform.position;
            myPosition.y = 0;

            Vector3 targetPosition = lookTarget;
            targetPosition.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(targetPosition - myPosition);

            Vector3 myPositionY = _transform.position;
            myPosition.x = 0;
            myPosition.z = 0;

            Vector3 targetPositionY = lookTarget;
            targetPosition.x = 0;
            targetPosition.z = 0;

            Quaternion targetRotationY = Quaternion.LookRotation(targetPositionY - myPositionY);

            RotateToInstant(targetRotation, targetRotationY);
        }

        public void RotateToInstant(Quaternion targetRotation, Quaternion targetRotationY)
        {
            StopRotation();

            _transform.DORotateQuaternion(targetRotation, _duration.Value).SetEase(_ease.Value);

            if(_transformY != null)
                _transformY.DORotateQuaternion(targetRotationY, _duration.Value).SetEase(_ease.Value);
        }
        
        public void ResetYRotation()
        {
            if (_transformY != null)
                _transformY.DOLocalRotateQuaternion(Quaternion.identity, _duration.Value * 2f).SetEase(_ease.Value);
        }


        public void StopRotation()
        {
            DOTween.Kill(_transform);
            DOTween.Kill(_transformY);

            DOTween.Kill(this);
        }
    }
}
