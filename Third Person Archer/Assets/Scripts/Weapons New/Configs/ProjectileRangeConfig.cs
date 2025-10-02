using UnityEngine;

[CreateAssetMenu(menuName = "Weapons/Projectile Range Config", fileName = "ProjectileRangeConfig")]
public class ProjectileRangeConfig : ScriptableObject
{
    [Range(0f, 1f)] public float NoFalloffFraction = 0.5f; // e.g., 0..0.5R = full damage
    [Range(0.1f, 1f)] public float MinDamageFraction = 0.45f; // floor at max range
    public float DespawnRange = 1000f;   // optional despawn tail, e.g., 1.25R

    public float GetFalloffStart(float rangeMeters) => Mathf.Max(0f, NoFalloffFraction * rangeMeters);
    public float GetFalloffEnd(float rangeMeters)   => Mathf.Max(0f, rangeMeters);
    public float GetDespawnDistance() => DespawnRange;

    /// <summary>
    /// Returns a 0..1 damage factor based on traveled distance and the weapon's rangeMeters.
    /// Linear falloff: 1.0 until start; lerp to MinDamageFraction by end; clamped after.
    /// </summary>
    public float EvaluateDamageFactor(float distanceMeters, float rangeMeters)
    {
        float start = GetFalloffStart(rangeMeters);
        float end   = GetFalloffEnd(rangeMeters);

        if (distanceMeters <= start) return 1f;
        if (distanceMeters >= end)   return Mathf.Clamp01(MinDamageFraction);

        float t = Mathf.InverseLerp(start, end, distanceMeters);
        return Mathf.Lerp(1f, Mathf.Clamp01(MinDamageFraction), t);
    }
}

