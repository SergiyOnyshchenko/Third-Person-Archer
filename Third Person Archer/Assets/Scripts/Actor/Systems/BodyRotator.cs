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
            // --- Горизонтальный поворот (parent, ось Y) ---
            Vector3 myPosition = _transform.position;
            Vector3 direction = lookTarget - myPosition;
            direction.y = 0;

            if (direction.sqrMagnitude < 0.0001f) return;

            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // --- Вертикальный поворот (child, локальный pitch по X) ---
            Quaternion targetRotationY = Quaternion.identity;

            if (_transformY != null)
            {
                Vector3 fullDirection = lookTarget - _transformY.position;
                float pitch = -Mathf.Atan2(fullDirection.y, new Vector2(fullDirection.x, fullDirection.z).magnitude) * Mathf.Rad2Deg;
                targetRotationY = Quaternion.Euler(pitch, 0f, 0f);
            }

            RotateToInstant(targetRotation, targetRotationY);
        }

        public void RotateToInstant(Quaternion targetRotation, Quaternion targetRotationY)
        {
            StopRotation();

            _transform.DORotateQuaternion(targetRotation, _duration.Value)
                .SetEase(_ease.Value)
                .SetTarget(_transform);

            if (_transformY != null)
                _transformY.DOLocalRotateQuaternion(targetRotationY, _duration.Value)
                    .SetEase(_ease.Value)
                    .SetTarget(_transformY);
        }

        public void ResetYRotation()
        {
            if (_transformY != null)
            {
                DOTween.Kill(_transformY);  // ← убиваем перед новым твином
                _transformY.DOLocalRotateQuaternion(Quaternion.identity, _duration.Value * 2f)
                    .SetEase(_ease.Value)
                    .SetTarget(_transformY);
            }
        }

        public void StopRotation()
        {
            DOTween.Kill(_transform);
            DOTween.Kill(_transformY);
        }
    }
}
