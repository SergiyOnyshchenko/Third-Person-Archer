using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class ProjectileLastHitData : Property
    {
        public Collision Value { get; private set; }

        public void SetValue(Collision value)
        {
            Value = value;
        }
    }
}