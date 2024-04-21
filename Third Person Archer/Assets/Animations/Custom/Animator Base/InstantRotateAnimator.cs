using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation
{
    public class InstantRotateAnimator<T, D> : Animator<T, D> where T : ITransform where D : IRotateAnimationData
    {
        public override void DoPose(T target, D data)
        {
            target.Transform.localRotation = data.Rotation;
        }

        protected override AnimatorType GetAnimatorType() => AnimatorType.Instant;
    }
}
