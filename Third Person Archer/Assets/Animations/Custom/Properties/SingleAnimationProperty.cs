using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SingleAnimationProperty<T> : AnimationProperty
{
    [SerializeField] protected T _value;
    public T Value { get => _value; }

    public virtual void Init(T value)
    {
        _value = value;
    } 

    public void SetValue(T value)
    {
        _value = value;
    }
}
