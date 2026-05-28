using Meta.Weapons;

public sealed class MissionGateResult
{
    public bool Passed { get; }
    public float RequiredDamage { get; }
    public float CurrentDamage { get; }
    public WeaponClass RequiredWeaponClass { get; }

    /// <summary>
    /// True if the currently equipped weapon can reach RequiredDamage through upgrades alone.
    /// False means the player must buy a higher-tier weapon (forced purchase gate).
    /// </summary>
    public bool CanUpgradeToPass { get; }

    public MissionGateResult(bool passed, float requiredDamage, float currentDamage, WeaponClass requiredWeaponClass, bool canUpgradeToPass = true)
    {
        Passed = passed;
        RequiredDamage = requiredDamage;
        CurrentDamage = currentDamage;
        RequiredWeaponClass = requiredWeaponClass;
        CanUpgradeToPass = canUpgradeToPass;
    }
}