using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Actor
{
    public interface IShootEvent
    {
        event Action OnShooted;
        event Action<HitInfo> OnHitResult;
    }
}