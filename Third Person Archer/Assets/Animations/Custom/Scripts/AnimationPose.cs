using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPose<T> : IAnimationPose<T> where T : class
{
    public T[] Instances { get; set; }
}
