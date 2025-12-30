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

    public DamageGatePopupArgs(
        WeaponClass weaponClass,
        int campaignLevel,
        float currentDamage,
        float requiredDamage,
        string weaponScreenId)
    {
        WeaponClass = weaponClass;
        CampaignLevel = campaignLevel;
        CurrentDamage = currentDamage;
        RequiredDamage = requiredDamage;
        WeaponScreenId = weaponScreenId;
    }
}