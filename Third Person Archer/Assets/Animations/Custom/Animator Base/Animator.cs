using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation
{
    public abstract class Animator<T, D> : MonoBehaviour
    {
        public AnimatorType Type { get => GetAnimatorType(); }
        public abstract void DoPose(T target, D data);
        protected abstract AnimatorType GetAnimatorType();
    }
}
