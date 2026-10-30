using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeScaleUtils
{
    /// <summary>
    /// Real-time frame delta for player-facing aim (camera, draw, zoom) so slow-mo doesn't slow the player's hands.
    /// 0 while the game is paused (timescale 0).
    /// </summary>
    public static float AimDeltaTime => Time.timeScale > 0f ? Time.unscaledDeltaTime : 0f;

    /// <summary>Frame-rate independent lerp factor for exponential smoothing at the given rate (1/s).</summary>
    public static float Damp(float rate, float deltaTime) => 1f - Mathf.Exp(-rate * deltaTime);

    public static float CalculateUnscaledSpeed(float desiredMetersPerSecond)
    {
        float ts = Mathf.Max(Time.timeScale, 0.0001f);
        float fdt = Mathf.Max(Time.fixedDeltaTime, 0.000001f);
        return desiredMetersPerSecond / (ts * fdt);
    }
}
