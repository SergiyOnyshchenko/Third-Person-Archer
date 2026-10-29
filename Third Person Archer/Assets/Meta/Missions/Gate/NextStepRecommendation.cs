using Meta.Weapons;

public enum NextStepRecommendationType
{
    None,
    UpgradeWeapon,   // player can afford to upgrade equipped weapon right now
    BuyWeapon,       // player can afford to buy a stronger weapon right now
    ReplayLevel,     // player needs gold/essence of the class: replay its last cleared level
}

/// <summary>
/// Result of <see cref="NextStepRecommendationService.Evaluate"/>.
/// Describes the most useful action when the player is blocked on a gate.
/// Always check <see cref="Type"/> first; all other fields are only meaningful
/// when Type is not None.
/// </summary>
public sealed class NextStepRecommendation
{
    public static readonly NextStepRecommendation None =
        new NextStepRecommendation(NextStepRecommendationType.None, default, 0f, 0f);

    public NextStepRecommendationType Type { get; }
    public WeaponClass RequiredWeaponClass { get; }
    public float CurrentDamage { get; }
    public float RequiredDamage { get; }

    // Set for UpgradeWeapon (equipped weapon id) and BuyWeapon (weapon to purchase id).
    public string TargetWeaponId { get; }

    // Set for ReplayLevel.
    public MissionType TargetMissionType { get; }

    // True when the gate is Crossbow-based (Boss gate or a Sniper level).
    public bool IsCrossbowPath { get; }

    public NextStepRecommendation(
        NextStepRecommendationType type,
        WeaponClass requiredWeaponClass,
        float currentDamage,
        float requiredDamage,
        string targetWeaponId = null,
        MissionType targetMissionType = default,
        bool isCrossbowPath = false)
    {
        Type = type;
        RequiredWeaponClass = requiredWeaponClass;
        CurrentDamage = currentDamage;
        RequiredDamage = requiredDamage;
        TargetWeaponId = targetWeaponId;
        TargetMissionType = targetMissionType;
        IsCrossbowPath = isCrossbowPath;
    }
}
