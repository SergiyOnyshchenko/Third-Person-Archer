using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GraphicsTier
{
    Low = 0,
    Medium = 1,
    High = 2
}

public enum GraphicsPreference
{
    Auto = 0,
    Low = 1,
    Medium = 2,
    High = 3
}

/// <summary>
/// Picks one of the three quality levels (Low / Medium / High in Quality Settings, each with its own URP asset):
/// either the one the player chose in the settings, or the one detected for this device.
/// </summary>
public static class GraphicsQuality
{
    private const string PreferenceKey = "gfx_preference";
    // Bump the suffix to re-detect every device after the detection rules change.
    private const string AutoTierKey = "gfx_auto_tier_v1";

    public static GraphicsTier Tier { get; private set; } = GraphicsTier.Medium;
    public static GraphicsPreference Preference => (GraphicsPreference)PlayerPrefs.GetInt(PreferenceKey, (int)GraphicsPreference.Auto);
    public static event Action<GraphicsTier> OnTierChanged;

    public static GraphicsTier AutoTier
    {
        get
        {
            int saved = PlayerPrefs.GetInt(AutoTierKey, -1);
            if (saved < 0)
            {
                saved = (int)GraphicsTierDetector.Detect();
                PlayerPrefs.SetInt(AutoTierKey, saved);
                PlayerPrefs.Save();
            }
            return (GraphicsTier)saved;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Apply(ResolveTier());
        GraphicsRuntime.Create();
    }

    public static void SetPreference(GraphicsPreference preference)
    {
        PlayerPrefs.SetInt(PreferenceKey, (int)preference);
        PlayerPrefs.Save();
        Apply(ResolveTier());
        GraphicsSceneLook.Apply(SceneManager.GetActiveScene());
    }

    /// <summary>The device can't hold the frame rate: step the detected level down. Takes effect from the next scene.</summary>
    public static bool LowerAutoTier()
    {
        if (Preference != GraphicsPreference.Auto || AutoTier == GraphicsTier.Low)
            return false;

        PlayerPrefs.SetInt(AutoTierKey, (int)AutoTier - 1);
        PlayerPrefs.Save();
        return true;
    }

    public static GraphicsTier ResolveTier()
    {
        return Preference == GraphicsPreference.Auto ? AutoTier : (GraphicsTier)((int)Preference - 1);
    }

    public static void Apply(GraphicsTier tier)
    {
        Tier = tier;

        int index = Array.IndexOf(QualitySettings.names, tier.ToString());
        if (index < 0)
            index = Mathf.Clamp((int)tier, 0, QualitySettings.names.Length - 1);

        if (QualitySettings.GetQualityLevel() != index)
            QualitySettings.SetQualityLevel(index, true);

        OnTierChanged?.Invoke(tier);
    }
}
