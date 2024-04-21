using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MyAnimator<T> : MonoBehaviour
{
    private AnimationProperty[] _properties;

    public abstract void DoPose(IAnimationPose<T> pose);
    public abstract T[] GetPoseData();

    public bool TryGetProperty(out AnimationProperty property)
    {
        if(_properties.TryGetProperty(out property))
            return true;

        return false;
    }   

    protected void InitProperties(AnimationProperty[] properties)
    {
        _properties = properties;
    }
}
