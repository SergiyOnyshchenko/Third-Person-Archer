using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace CustomAnimation
{
    [System.Serializable]
    public class Animation<T, D> where D : class
    {
        [SerializeField] private AnimationPoseData<D>[] _poses;
        [SerializeField] private AnimatorController<T, D> _animator;

        private Tween _playTween;

        public void Init(AnimatorController<T, D> animator) => _animator = animator;

        public void Play(float duration, bool ignoreTimeScale, UnityAction onCompleted)
        {
            _playTween?.Kill();
            _playTween = DOVirtual.Float(0f, 1f, duration, v =>
            {
                var pose = GetAnimationPose(v);
                if (pose != null)
                    _animator.DoPose(pose);
            })
            .SetUpdate(ignoreTimeScale)
            .OnComplete(() => onCompleted?.Invoke());
        }

        public IAnimationPose<D> GetAnimationPose(float t)
        {
            if (_poses == null || _poses.Length == 0) return null;
            if (_poses.Length == 1) return _poses[0];

            t = Mathf.Clamp01(t);

            // Map normalized time to fractional index
            float f = t * (_poses.Length - 1);    // e.g., 0..(N-1)
            int prev = Mathf.FloorToInt(f);       // 0..N-2
            int next = Mathf.Min(prev + 1, _poses.Length - 1);
            float localT = f - prev;              // 0..1 within segment

            return _animator.LerpPoses(_poses[prev], _poses[next], localT);
        }
    }
}