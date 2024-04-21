using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringVector3 : SpringValue<Vector3>
{
    public SpringVector3(float power, float dumping, Vector3 value = default, Vector3 velocity = default) : base(power, dumping, value, velocity)
    {
    }

    public override Vector3 UpdateValue(Vector3 targetValue)
    {
        SpringCalculator.UpdateDampedSpringMotion(ref _value, ref _velocity, targetValue, _springParams);
        return _value;
    }
}
