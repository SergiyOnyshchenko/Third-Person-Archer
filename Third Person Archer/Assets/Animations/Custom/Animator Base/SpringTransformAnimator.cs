using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation
{
    public class SpringTransformAnimator<T, D> : Animator<T, D> where T : ISpringMotion where D : ITransformAnimationData
    {
        private SpringPower _springPower;
        private SpringDumping _springDumping;

        public void InitParams(SpringPower springPower, SpringDumping springDumping)
        {
            _springPower = springPower;
            _springDumping = springDumping;
        }

        public override void DoPose(T target, D data)
        {
            target.SetSpringParams(_springPower.Value, _springDumping.Value);

            target.SetTargetPosition(data.Position);
            target.SetTargetRotation(data.Rotation);
        }

        protected override AnimatorType GetAnimatorType() => AnimatorType.Spring;
    }
}
