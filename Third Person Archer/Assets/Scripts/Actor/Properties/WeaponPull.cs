using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Actor.Properties;

public class WeaponPull : FloatProperty
{
    public UnityEvent OnPullBegin = new UnityEvent();
    public UnityEvent OnPullRelease = new UnityEvent();

    public void BeginPull()
    {
        SetPullValue(0);
        OnPullBegin?.Invoke();
    }

    public void ReleasePull()
    {
        OnPullRelease?.Invoke();
    }

    public void SetPullValue(float value)
    {
        value = Mathf.Clamp01(value);
        SetValue(value);
    }
}
