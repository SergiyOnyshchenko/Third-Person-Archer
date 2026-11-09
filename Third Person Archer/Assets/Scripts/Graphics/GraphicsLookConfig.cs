using System;
using UnityEngine;

/// <summary>
/// Tuning of the per-scene look applied on top of each quality level. Lives in Resources as "Graphics Look".
/// </summary>
[CreateAssetMenu(fileName = "Graphics Look", menuName = "Graphics/Look Config")]
public class GraphicsLookConfig : ScriptableObject
{
    [Serializable]
    public class ZoneGrade
    {
        public string Name;
        [Tooltip("The grade is used when the scene name contains any of these.")]
        public string[] SceneTokens;
        [Range(-100f, 100f)] public float Contrast = 10f;
        [Range(-100f, 100f)] public float Saturation = 10f;
        public float PostExposure = 0.15f;
        public Color ColorFilter = Color.white;
    }

    [Header("Ambient (all levels)")]
    [Tooltip("The scene's flat ambient color becomes a sky / horizon / ground gradient with these multipliers.")]
    public float SkyMultiplier = 1.05f;
    public float EquatorMultiplier = 0.8f;
    public float GroundMultiplier = 0.6f;
    [Tooltip("The ground part leans towards this earthy color.")]
    public Color GroundTint = new Color(0.45f, 0.38f, 0.35f);
    [Range(0f, 1f)] public float GroundTintAmount = 0.5f;

    [Header("Shadows (Medium, High)")]
    [Tooltip("Static objects smaller than this (bounds diagonal, m) stop casting shadows. Characters always cast.")]
    public float MinCasterSize = 1.5f;

    [Header("Vignette (all levels)")]
    [Range(0f, 1f)] public float VignetteIntensity = 0.2f;
    [Range(0.01f, 1f)] public float VignetteSmoothness = 0.5f;

    [Header("High only")]
    public float BloomIntensity = 0.5f;
    public float BloomThreshold = 1f;
    [Range(0f, 1f)] public float BloomScatter = 0.6f;
    [Tooltip("Checked in order, the first match wins, so night goes before the zones.")]
    public ZoneGrade[] Grades = Array.Empty<ZoneGrade>();
    public ZoneGrade DefaultGrade = new ZoneGrade { Name = "Default", Contrast = 8f, Saturation = 8f };

    private static GraphicsLookConfig _instance;

    public static GraphicsLookConfig Instance
    {
        get
        {
            if (_instance == null)
                _instance = Resources.Load<GraphicsLookConfig>("Graphics Look");
            if (_instance == null)
                _instance = CreateInstance<GraphicsLookConfig>();
            return _instance;
        }
    }

    public ZoneGrade GetGrade(string sceneName)
    {
        foreach (var grade in Grades)
            foreach (var token in grade.SceneTokens)
                if (!string.IsNullOrEmpty(token) && sceneName.Contains(token))
                    return grade;

        return DefaultGrade;
    }
}
