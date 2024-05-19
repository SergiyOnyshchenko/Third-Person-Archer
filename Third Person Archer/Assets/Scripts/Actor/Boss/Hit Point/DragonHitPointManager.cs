using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class DragonHitPointManager : HitPointManager
    {
        [SerializeField] private HitPoint _leftEye;
        [SerializeField] private HitPoint _rightEye;
        [SerializeField] private HitPoint _nose;
        [Space]
        [SerializeField] private HitPoint _bodySpine1;
        [SerializeField] private HitPoint _bodySpine2;
        [SerializeField] private HitPoint _tale;
        [Space]
        [SerializeField] private HitPoint _leftHand;
        [SerializeField] private HitPoint _rightHand;
        [Space]
        [SerializeField] private HitPoint _leftLeg;
        [SerializeField] private HitPoint _rightLeg;

        private void Awake()
        {
            InitHitPoints();
        }

        private void InitHitPoints()
        {
            _hitPoints = new HitPoint[]
            {
                _leftEye,
                _rightEye,
                _nose,
                _bodySpine1,
                _bodySpine2,
                _tale,
                _leftHand,
                _rightHand,
                _leftLeg,
                _rightLeg
            };

            foreach (HitPoint hitPoint in _hitPoints)
                hitPoint.Desactivate();
        }
    }
}
