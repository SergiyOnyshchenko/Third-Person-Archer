using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public class FpvInput : Input
    {
        public float Horizontal { get; protected set; }
        public float Vertical { get; protected set; }
    }
}