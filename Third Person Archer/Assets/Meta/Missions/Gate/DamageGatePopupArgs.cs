using System;
using Meta.Weapons;
using Meta.Weapons.UI;

public enum GatePopupAction
{
    OpenWeapons,    // open WeaponSelectionScreen (default)
    PlayContracts,  // direct-start a Contracts mission
    PlaySniper,     // direct-start a Sniper mission
}

[Serializable]
public sealed class DamageGatePopupArgs
{
    public WeaponClass WeaponClass { get; }
    public int CampaignLevel { get; }

    public float CurrentDamage { get; }
    public float RequiredDamage { get; }

    public string WeaponScreenId { get; }

    /// <summary>True if the equipped weapon can be upgraded to reach RequiredDamage.</summary>
    public bool CanUpgradeToPass { get; }

    // ── Action routing (Phase 4) ──────────────────────────────────────────────
    public GatePopupAction Action { get; }

    /// <summary>Weapon to pre-select when Action = OpenWeapons. Null = default (equipped).</summary>
    public string PreselectWeaponId { get; }

    /// <summary>Button highlight when Action = OpenWeapons.</summary>
    public WeaponHighlightMode HighlightMode { get; }

    // ── Legacy fields kept for backward compatibility; no longer used by PlayMissionPresenter ──

    /// <summary>Unused since Phase 4. Kept so serialized assets remain valid.</summary>
    public string CustomTitle { get; }

    /// <summary>Unused since Phase 4.</summary>
    public string CustomHint { get; }

    /// <summary>Unused since Phase 4. Tutorial block is no longer shown.</summary>
    public string TutorialText { get; }

    // ── Primary constructor (Phase 4) ─────────────────────────────────────────

    public DamageGatePopupArgs(
        WeaponClass weaponClass,
        int campaignLevel,
        float currentDamage,
        float requiredDamage,
        string weaponScreenId,
        GatePopupAction action,
        bool canUpgradeToPass = true,
        string preselectWeaponId = null,
        WeaponHighlightMode highlightMode = WeaponHighlightMode.None)
    {
        WeaponClass = weaponClass;
        CampaignLevel = campaignLevel;
        CurrentDamage = currentDamage;
        RequiredDamage = requiredDamage;
        WeaponScreenId = weaponScreenId;
        Action = action;
        CanUpgradeToPass = canUpgradeToPass;
        PreselectWeaponId = preselectWeaponId;
        HighlightMode = highlightMode;
    }
}
