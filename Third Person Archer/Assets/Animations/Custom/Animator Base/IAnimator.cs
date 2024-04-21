using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation
{
    public interface IAnimator<T, D>
    {
        public AnimatorType Type { get; }
        public void DoPose(T target, D data);
        public AnimatorType GetAnimatorType();
    }
}