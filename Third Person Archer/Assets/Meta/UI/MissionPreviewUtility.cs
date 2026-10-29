using UnityEngine;

/// <summary>
/// Shared numbers and colors for the map path, the mission panel and the victory screen.
/// </summary>
public static class MissionPreviewUtility
{
    public static readonly Color Green = new Color(0.45f, 0.85f, 0.32f);
    public static readonly Color Yellow = new Color(1f, 0.78f, 0.22f);
    public static readonly Color Red = new Color(0.93f, 0.3f, 0.24f);
    public static readonly Color Gold = new Color(1f, 0.84f, 0.38f);
    public static readonly Color Heroic = new Color(1f, 0.38f, 0.3f);

    /// <summary>Heroic accent that stays readable on parchment.</summary>
    public static Color GetInkHeroicColor() => new Color(0.6f, 0.08f, 0.14f);

    /// <summary>Darker status colors that stay readable on parchment.</summary>
    public static Color GetInkStatusColor(MissionGateStatus status) => status switch
    {
        MissionGateStatus.Green => new Color(0.16f, 0.48f, 0.1f),
        MissionGateStatus.Yellow => new Color(0.72f, 0.4f, 0.02f),
        _ => new Color(0.72f, 0.12f, 0.08f)
    };

    public static Color GetStatusColor(MissionGateStatus status) => status switch
    {
        MissionGateStatus.Green => Green,
        MissionGateStatus.Yellow => Yellow,
        _ => Red
    };

    public static string Hex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);
}
