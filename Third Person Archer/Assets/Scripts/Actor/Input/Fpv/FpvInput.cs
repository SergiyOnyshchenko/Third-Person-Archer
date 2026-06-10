using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class FpvInput : Input
    {
        public float Horizontal { get; protected set; }
        public float Vertical { get; protected set; }
        public bool IsFrozen { get; protected set; }
        public float SensitivityMultiplier { get; set; } = 1f;

        public void Activate (bool value)
        {
            IsActive = value;
        }

        public void Freeze(bool value)
        {
            IsFrozen = value;
        }
    }
}