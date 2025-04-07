using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class FpvInput : Input
    {
        public virtual float Horizontal { get; protected set; }
        public virtual float Vertical { get; protected set; }
        public virtual bool IsFrozen { get; protected set; }

        public void Freeze(bool value)
        {
            IsFrozen = value;
        }
    }
}