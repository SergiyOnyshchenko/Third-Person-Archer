using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Actor.Properties;

public class WeaponPull : FloatProperty, IPull
{
    public UnityEvent OnPullBegin = new UnityEvent();
    public UnityEvent OnPullRelease = new UnityEvent();
    public float PullPower => Value;
    public bool IsPulling { get; private set; }

    public void BeginPull()
    {
        IsPulling = true;
        SetPullValue(0);
        OnPullBegin?.Invoke();
    }

    public void ReleasePull()
    {
        IsPulling = false;
        SetPullValue(0);
        OnPullRelease?.Invoke();
    }

    public void SetPullValue(float value)
    {
        value = Mathf.Clamp01(value);
        SetValue(value);
    }
}
