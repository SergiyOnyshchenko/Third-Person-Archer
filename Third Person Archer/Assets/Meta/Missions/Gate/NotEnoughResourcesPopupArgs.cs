using Meta.Weapons;

/// <summary>
/// Args for NotEnoughResourcesPopupController.
/// Shown when the next gate is blocked and the player has no affordable upgrade/buy
/// option — they replay a cleared level of the class for gold and essence.
/// </summary>
public sealed class NotEnoughResourcesPopupArgs
{
    /// <summary>Class of the blocked level: the replay gives its essence.</summary>
    public WeaponClass WeaponClass { get; }

    public float CurrentDamage { get; }
    public float RequiredDamage { get; }

    public NotEnoughResourcesPopupArgs(WeaponClass weaponClass, float currentDamage, float requiredDamage)
    {
        WeaponClass = weaponClass;
        CurrentDamage = currentDamage;
        RequiredDamage = requiredDamage;
    }
}
