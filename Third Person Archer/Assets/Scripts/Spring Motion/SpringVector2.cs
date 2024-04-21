using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpringVector2 : SpringValue<Vector2>
{
    public SpringVector2(float power, float dumping, Vector2 value = default, Vector2 velocity = default) : base(power, dumping, value, velocity)
    {
    }

    public override Vector2 UpdateValue(Vector2 targetValue)
    {
        SpringCalculator.UpdateDampedSpringMotion(ref _value, ref _velocity, targetValue, _springParams);
        return _value;
    }
}
