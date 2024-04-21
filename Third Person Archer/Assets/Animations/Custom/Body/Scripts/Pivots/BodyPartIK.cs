using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace CustomAnimation.Body
{
    public class BodyPartIK : MonoBehaviour, ITransform, ISpringMotion
    {
        [SerializeField] public bool UseSpring;
        [Space]
        [SerializeField] private BodyNameIK _name;
        [SerializeField] private Transform _ikPoint;
        [Space]
        [SerializeField] private Color _debugColor = Color.red;
        private SpringMotion _springMotion;
        public BodyNameIK Name { get => _name; }
        public Transform Transform => transform;
        public Transform IkPoint { get => _ikPoint; }

        private void Awake()
        {
            _springMotion = new SpringMotion();
            _springMotion.SetTargetPosition(transform.localPosition);
            _springMotion.SetTargetRotation(transform.localRotation);
            _springMotion.SetSpringParams(2.5f, 0.25f);
        }

        private void Update()
        {
            if(UseSpring)
                _springMotion.Update(transform);
        }

        public void SetSpringParams(float power, float dumping) => _springMotion.SetSpringParams(power, dumping);
        public void SetTargetPosition(Vector3 position) => _springMotion.SetTargetPosition(position);
        public void SetTargetRotation(Quaternion rotation) => _springMotion.SetTargetRotation(rotation);

        public void Shake(float power)
        {
            power *= 1.5f;
            float duration = 1.25f * power;
            power = 0.025f;

            DOTween.Kill(this);
            DOTween.Kill(_ikPoint);

            _ikPoint.DOShakePosition(duration, power, 8, 90, false, true).OnComplete(() => _ikPoint.DOLocalMove(Vector3.zero, 0.25f));;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = _debugColor;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(Vector3.zero, Vector3.one * 0.1f);
        }
    }
}