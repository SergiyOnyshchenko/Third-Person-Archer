using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeScaleUtils
{
    public static float CalculateUnscaledSpeed(float desiredMetersPerSecond)
    {
        float ts = Mathf.Max(Time.timeScale, 0.0001f);
        float fdt = Mathf.Max(Time.fixedDeltaTime, 0.000001f);
        return desiredMetersPerSecond / (ts * fdt);
    }
}
