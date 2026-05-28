using System;
using Meta.Weapons;

[Serializable]
public sealed class DamageGatePopupArgs
{
    public WeaponClass WeaponClass { get; }
    public int CampaignLevel { get; } // company level (global campaign index + 1)

    public float CurrentDamage { get; }
    public float RequiredDamage { get; }

    public string WeaponScreenId { get; } // screen id in your ScreenRegistry

    /// <summary>
    /// True if the player can upgrade the current weapon to reach RequiredDamage.
    /// False = current weapon is at its maximum tier; player must buy a new one.
    /// </summary>
    public bool CanUpgradeToPass { get; }

    /// <summary>Optional title override. If null, the popup uses its serialized default.</summary>
    public string CustomTitle { get; }

    /// <summary>Optional hint override. If null, the popup selects hint based on CanUpgradeToPass.</summary>
    public string CustomHint { get; }

    public DamageGatePopupArgs(
        WeaponClass weaponClass,
        int campaignLevel,
        float currentDamage,
        float requiredDamage,
        string weaponScreenId,
        bool canUpgradeToPass = true,
        string customTitle = null,
        string customHint = null)
    {
        WeaponClass = weaponClass;
        CampaignLevel = campaignLevel;
        CurrentDamage = currentDamage;
        RequiredDamage = requiredDamage;
        WeaponScreenId = weaponScreenId;
        CanUpgradeToPass = canUpgradeToPass;
        CustomTitle = customTitle;
        CustomHint = customHint;
    }
}
