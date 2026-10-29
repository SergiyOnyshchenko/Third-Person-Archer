using Meta.Weapons;

/// <summary>
/// Campaign soft gate (1–2 shot rule):
/// Green  — damage >= recommended R: regular enemies die in 1 body shot.
/// Yellow — min <= damage < R: playable, some enemies need 2 body shots (head is always 1).
/// Red    — damage < min: blocked.
/// Hard gate (Boss) is only Green or Red.
/// </summary>
public enum MissionGateStatus
{
    Green,
    Yellow,
    Red,
}

public sealed class MissionGateResult
{
    public bool Passed => Status != MissionGateStatus.Red;
    public MissionGateStatus Status { get; }

    /// <summary>Minimum damage to start the mission (below = Red).</summary>
    public float RequiredDamage { get; }

    /// <summary>Damage for Green. Equals RequiredDamage for hard gates.</summary>
    public float RecommendedDamage { get; }

    public float CurrentDamage { get; }
    public WeaponClass RequiredWeaponClass { get; }

    /// <summary>
    /// True if the currently equipped weapon can reach RequiredDamage through upgrades alone.
    /// False means the player must buy a higher-tier weapon (forced purchase gate).
    /// </summary>
    public bool CanUpgradeToPass { get; }

    /// <summary>Hard gate: passes at requiredDamage.</summary>
    public MissionGateResult(bool passed, float requiredDamage, float currentDamage, WeaponClass requiredWeaponClass, bool canUpgradeToPass = true)
        : this(passed ? MissionGateStatus.Green : MissionGateStatus.Red,
               requiredDamage, requiredDamage, currentDamage, requiredWeaponClass, canUpgradeToPass)
    {
    }

    public MissionGateResult(MissionGateStatus status, float requiredDamage, float recommendedDamage,
                             float currentDamage, WeaponClass requiredWeaponClass, bool canUpgradeToPass = true)
    {
        Status = status;
        RequiredDamage = requiredDamage;
        RecommendedDamage = recommendedDamage;
        CurrentDamage = currentDamage;
        RequiredWeaponClass = requiredWeaponClass;
        CanUpgradeToPass = canUpgradeToPass;
    }
}
