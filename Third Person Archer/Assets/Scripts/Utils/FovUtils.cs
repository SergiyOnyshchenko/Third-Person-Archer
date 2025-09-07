using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class FovUtils
{
    public static float FovFromMagnification(float normalFovDeg, float magnification)
    {
        magnification = Mathf.Max(1f, magnification);
        float rad = Mathf.Deg2Rad;
        return 2f * Mathf.Atan(Mathf.Tan(0.5f * normalFovDeg * rad) / magnification) / rad;
    }
}