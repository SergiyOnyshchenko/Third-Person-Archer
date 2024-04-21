using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CustomAnimation
{
    public interface IActiveAnimation
    {
        public void Play();
        public void Stop();
        public void SetAnimationValue(float value);
    }
}