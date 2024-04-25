using System.Collections;
using System.Collections.Generic;
using CustomAnimation.Body;
using CustomAnimation.FPV;
using CustomAnimation.Hand;
using UnityEngine;

namespace Actor
{
    public class FpvController : System
    {
        [Header("IK Points")]
        [SerializeField] private BodyPartIK _leftHand;
        [SerializeField] private BodyPartIK _rightHand;
        [Header("Custom Animators")]
        [SerializeField] private FpvAnimatorController _fpvAnimator;
        [SerializeField] private HandAnimatorController _leftHandAnimator;
        [SerializeField] private HandAnimatorController _rightHandAnimator;
        public BodyPartIK LeftHand { get => _leftHand; }
        public BodyPartIK RightHand { get => _rightHand; }
        public FpvAnimatorController FpvAnimator { get => _fpvAnimator; }
        public HandAnimatorController LeftHandAnimator { get => _leftHandAnimator; }
        public HandAnimatorController RightHandAnimator { get => _rightHandAnimator; }

        public void ApplyHandsSpring(bool value)
        {
            LeftHand.ApplySpring(value);
            RightHand.ApplySpring(value);
        }
    }
}