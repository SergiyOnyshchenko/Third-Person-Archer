using UnityEngine;

/// <summary>Shots needed to kill one enemy type with the player's damage.</summary>
public readonly struct EnemyShotsPreview
{
    public readonly EnemyArchetype Type;
    public readonly int Hp;
    public readonly int BodyShots;
    public readonly int HeadShots;

    public EnemyShotsPreview(EnemyArchetype type, int hp, int bodyShots, int headShots)
    {
        Type = type;
        Hp = hp;
        BodyShots = bodyShots;
        HeadShots = headShots;
    }
}

/// <summary>
/// Shared numbers and colors for the map path, the mission panel and the victory screen.
/// Enemy HP mirrors BalanceConfig.GetEnemyStats for Campaign: floor(R * min(1, typeHp)).
/// </summary>
public static class MissionPreviewUtility
{
    public const float HeadshotMultiplier = 2f;

    public static readonly EnemyArchetype[] RosterTypes =
    {
        EnemyArchetype.Melee,
        EnemyArchetype.RangedPassive,
        EnemyArchetype.RangedActive,
    };

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

    public static string GetEnemyName(EnemyArchetype type) => type switch
    {
        EnemyArchetype.Melee => "Brawler",
        EnemyArchetype.RangedPassive => "Shooter",
        EnemyArchetype.RangedActive => "Hunter",
        _ => type.ToString()
    };

    public static EnemyShotsPreview GetShots(BalanceConfig balance, int recommended, int damage, EnemyArchetype type)
    {
        var profile = balance != null ? balance.GetEnemyProfile(type) : null;
        float typeHp = profile != null ? profile.HpMultiplier : 1f;

        int hp = Mathf.Max(1, Mathf.FloorToInt(recommended * Mathf.Min(1f, typeHp)));
        int body = damage > 0 ? Mathf.CeilToInt(hp / (float)damage) : 99;
        int head = damage > 0 ? Mathf.CeilToInt(hp / (damage * HeadshotMultiplier)) : 99;

        return new EnemyShotsPreview(type, hp, body, head);
    }

    public static Color GetStatusColor(MissionGateStatus status) => status switch
    {
        MissionGateStatus.Green => Green,
        MissionGateStatus.Yellow => Yellow,
        _ => Red
    };

    public static Color GetShotsColor(int shots) => shots <= 1 ? Green : shots == 2 ? Yellow : Red;

    public static string FormatShots(int shots) => shots >= 99 ? "-" : shots == 1 ? "1 shot" : $"{shots} shots";

    public static string Hex(Color c) => "#" + ColorUtility.ToHtmlStringRGB(c);
}
