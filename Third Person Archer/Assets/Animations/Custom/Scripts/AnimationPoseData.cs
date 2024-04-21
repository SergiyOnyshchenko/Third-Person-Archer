using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class AnimationPoseData<T> : ScriptableObject, IAnimationPose<T> where T : class
{
    [SerializeField] protected T[] _instances;
    public T[] Instances => _instances;

    public void Init(T[] instances)
    {
        _instances = instances;
    }
}

