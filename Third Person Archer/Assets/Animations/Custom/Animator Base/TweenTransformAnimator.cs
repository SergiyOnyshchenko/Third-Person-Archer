using System;
using DG.Tweening;

namespace CustomAnimation
{
    public class TweenTransformAnimator<T, D> : Animator<T, D> where T : ITransform where D : ITransformAnimationData
    {
        private Duration _duration;
        private EaseType _easeType;

        public void InitParams(Duration duration, EaseType easeType)
        {
            _duration = duration;
            _easeType = easeType;
        }

        public override void DoPose(T target, D data)
        {
            float duration = _duration.Value;
            Ease ease = _easeType.Value;

            target.Transform.DOLocalMove(data.Position, duration).SetEase(ease);
            target.Transform.DOLocalRotateQuaternion(data.Rotation, duration).SetEase(ease);
        }

        protected override AnimatorType GetAnimatorType() => AnimatorType.Tween;
    }
}
