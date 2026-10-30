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

    /// <summary>How much smaller one degree looks at <paramref name="fovDeg"/> than at <paramref name="normalFovDeg"/> (1 = same, &lt;1 = zoomed in).</summary>
    public static float ZoomRatio(float fovDeg, float normalFovDeg)
    {
        float rad = 0.5f * Mathf.Deg2Rad;
        return Mathf.Tan(fovDeg * rad) / Mathf.Max(0.0001f, Mathf.Tan(normalFovDeg * rad));
    }
}
