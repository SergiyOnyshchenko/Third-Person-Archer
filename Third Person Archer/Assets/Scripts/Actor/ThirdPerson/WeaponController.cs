using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.ThirdPerson
{
    public class WeaponController : System
    {
        [SerializeField] private Transform _target;
        [Space]
        [SerializeField] private Transform _weapon;
        [Space]
        [SerializeField] private Transform _idlePoint;
        [SerializeField] private Transform _firePoint;
        private SpringMotion _motion;
        private Transform _currentPoint;

        private void Start()
        {
            _motion = new SpringMotion(3f, 0.5f);
            SetIndle();
        }

        private void FuxedUpdate()
        {
            _motion.Update(_weapon);

            if (_target == null)
                return;

            _weapon.LookAt(_target.position + (Vector3.up * 1.5f));
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }

        public void SetIndle() => SetPoint(_idlePoint);

        public void SetFire() => SetPoint(_firePoint);

        private void SetPoint(Transform point)
        {
            _currentPoint = point;

            _motion.SetTargetPosition(_currentPoint.localPosition);
            _motion.SetTargetRotation(_currentPoint.localRotation);
        }

    }
}