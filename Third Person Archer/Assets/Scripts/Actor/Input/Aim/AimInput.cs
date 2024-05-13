using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor
{
    public abstract class AimInput : Input
    {
        public abstract Vector3 GetAimDirection();
        public abstract Transform GetAimTarget();
    }
}
