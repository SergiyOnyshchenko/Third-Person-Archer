using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Actor
{
    public interface ITarget
    {
        Transform TargetPoint { get; }
        Transform RootPoint { get; }
        IDamageable Damageable { get; }
        bool IsDead { get; }
        event Action<ITarget> OnDied;
    }
}
