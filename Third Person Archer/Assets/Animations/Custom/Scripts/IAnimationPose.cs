using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAnimationPose<I>
{
    I[] Instances { get; }
}
