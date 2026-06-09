using Meta.Weapons;

/// <summary>
/// Args for WeaponTooWeakPopupController.
/// Shown when the next gate is blocked and the player can act on the weapon screen
/// right now (upgrade or buy).
/// </summary>
public sealed class WeaponTooWeakPopupArgs
{
    public WeaponClass RequiredWeaponClass { get; }
    public float CurrentDamage { get; }
    public float RequiredDamage { get; }

    /// <summary>True = UpgradeWeapon recommendation. False = BuyWeapon recommendation.</summary>
    public bool IsUpgrade { get; }

    /// <summary>Weapon ID to preselect in WeaponSelectionScreen.</summary>
    public string TargetWeaponId { get; }

    /// <summary>ScreenRegistry ID of WeaponSelectionScreen.</summary>
    public string WeaponScreenId { get; }

    /// <summary>Current company level — passed to WeaponSelectionScreen for unlock filtering.</summary>
    public int CampaignLevel { get; }

    public WeaponTooWeakPopupArgs(
        WeaponClass requiredWeaponClass,
        float currentDamage,
        float requiredDamage,
        bool isUpgrade,
        string targetWeaponId,
        string weaponScreenId,
        int campaignLevel)
    {
        RequiredWeaponClass = requiredWeaponClass;
        CurrentDamage = currentDamage;
        RequiredDamage = requiredDamage;
        IsUpgrade = isUpgrade;
        TargetWeaponId = targetWeaponId;
        WeaponScreenId = weaponScreenId;
        CampaignLevel = campaignLevel;
    }
}
