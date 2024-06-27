using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPull
{
    public float PullPower { get; }
    public bool IsPulling { get; }
}
