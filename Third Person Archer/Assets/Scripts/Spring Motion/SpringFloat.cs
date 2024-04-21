using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringFloat : SpringValue<float>
{
    public SpringFloat(float power, float dumping, float value = 0, float velocity = 0) : base(power, dumping, value, velocity){}

    public override float UpdateValue(float targetValue)
    {
        SpringCalculator.UpdateDampedSpringMotion(ref _value, ref _velocity, targetValue, _springParams);
        return _value;
    }
}
