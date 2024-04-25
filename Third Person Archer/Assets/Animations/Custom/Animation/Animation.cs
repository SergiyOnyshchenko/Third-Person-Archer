using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using static UnityEngine.Rendering.DebugUI;

namespace CustomAnimation
{
    [System.Serializable]
    public class Animation<T, D> where D : class
    {
        [SerializeField] private AnimationPoseData<D>[] _poses;
        [SerializeField] private AnimatorController<T, D> _animator;

        public void Init(AnimatorController<T, D> animator)
        {
            _animator = animator;
        }

        public void Play(float duration)
        {
            DOVirtual.Float(0f, 1f, duration, v =>
            {
                _animator.DoPose(GetAnimationPose(v));
            });
        }

        public IAnimationPose<D> GetAnimationPose(float value)
        {
            int previousPose = 0;
            int nextPose = 0;

            float segment = 1f / _poses.Length;

            for (int i = 0; i < _poses.Length; i++)
            {
                nextPose = i;

                if (value <= i * segment)
                    break;

                previousPose = i;
            }

            value = Mathf.InverseLerp(previousPose, nextPose, value);

            return _animator.LerpPoses(_poses[previousPose], _poses[nextPose], value);
        }
    }
}

