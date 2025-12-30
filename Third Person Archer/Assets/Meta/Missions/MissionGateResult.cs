using Meta.Weapons;

public sealed class MissionGateResult
{
    public bool Passed { get; }
    public float RequiredDamage { get; }
    public float CurrentDamage { get; }
    public WeaponClass RequiredWeaponClass { get; }

    public MissionGateResult(bool passed, float requiredDamage, float currentDamage, WeaponClass requiredWeaponClass)
    {
        Passed = passed;
        RequiredDamage = requiredDamage;
        CurrentDamage = currentDamage;
        RequiredWeaponClass = requiredWeaponClass;
    }
}