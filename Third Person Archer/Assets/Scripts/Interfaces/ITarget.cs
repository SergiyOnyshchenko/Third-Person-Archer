using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITarget 
{
    Transform TargetPoint { get; }
    bool IsDead { get; }
}
