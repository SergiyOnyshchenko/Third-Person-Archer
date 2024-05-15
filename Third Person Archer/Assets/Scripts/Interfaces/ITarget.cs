using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ITarget 
{
    Transform TargetPoint { get; }
    Transform RootPoint { get; }
    bool IsDead { get; }
}
