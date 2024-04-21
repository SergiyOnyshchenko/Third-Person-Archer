using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation
{
    public class InstantTransformAnimator<T, D> : Animator<T, D> where T : ITransform where D : ITransformAnimationData 
    {
        public override void DoPose(T target, D data)
        {
            target.Transform.localPosition = data.Position;
            target.Transform.localRotation = data.Rotation;
        }

        protected override AnimatorType GetAnimatorType() => AnimatorType.Instant;
    }
}
