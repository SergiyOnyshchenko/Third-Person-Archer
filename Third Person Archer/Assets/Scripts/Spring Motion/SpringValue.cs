using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SpringValue<T>
{
    protected T _value;
    protected T _velocity;
    protected float _power;
    protected float _dumping;
    protected DampedSpringMotionParams _springParams;
    public T Value { get => _value; }
    public T Velocity { get => _velocity; }

    public SpringValue(float power, float dumping, T value = default, T velocity = default)
    {
        SetParams(power, dumping);
        _value = value;
        _velocity = velocity;
    }

    public abstract T UpdateValue(T targetValue);

    public void SetParams(float power, float dumping)
    {
        _power = power;
        _dumping = dumping;
        SpringCalculator.CalcDampedSpringMotionParams(ref _springParams, Time.fixedDeltaTime, _power, _dumping);
    }
}
