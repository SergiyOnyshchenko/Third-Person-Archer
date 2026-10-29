using System;

namespace Meta.Weapons
{
    public enum TierPurchaseBlock
    {
        None,
        AlreadyOwned,
        NotATier,             // skins are unlocked with ads
        CampaignLocked,       // UnlockAfterCampaignLevel not reached
        PreviousTierMissing,
        PreviousTierNotMaxed, // class level must reach the cap of the current tier first
        NotEnoughCurrency,
    }

    /// <summary>
    /// Per-class progression: the class owns the upgrade level, the best owned tier sets the level cap
    /// and the damage range, skins are cosmetic.
    /// Tier n covers class levels [start(n) .. start(n) + MaxUpgradeLevel(n)], start(n) = sum of lower tiers' MaxUpgradeLevel.
    /// </summary>
    public interface IClassProgressionService
    {
        /// <summary>Raised after a tier purchase, a skin unlock or a class level change.</summary>
        event Action<WeaponClass> OnProgressChanged;

        int GetClassLevel(WeaponClass cls);
        int GetLevelCap(WeaponClass cls);
        bool IsAtLevelCap(WeaponClass cls);

        /// <summary>Highest owned tier of the class (gives the power), or null.</summary>
        WeaponDef GetPowerDef(WeaponClass cls);

        /// <summary>Lowest tier above the power tier, or null.</summary>
        WeaponDef GetNextTierDef(WeaponClass cls);

        WeaponDef GetTierDef(WeaponClass cls, int tier);
        int GetTierStartLevel(WeaponDef tierDef);

        /// <summary>Stats the class shoots with (power tier at the class level).</summary>
        WeaponStats GetPowerStats(WeaponClass cls);

        /// <summary>Stats of a tier at a class level (clamped into the tier's range).</summary>
        WeaponStats GetTierStats(WeaponDef tierDef, int classLevel);

        bool IsOwned(string weaponId);

        TierPurchaseBlock GetTierPurchaseBlock(WeaponDef def, int companyLevel);
        bool TryBuyTier(WeaponDef def, int companyLevel);

        int GetSkinAdsWatched(WeaponDef skin);

        /// <summary>Counts one watched ad; unlocks the skin when enough are watched. Returns true if now owned.</summary>
        bool RegisterSkinAd(WeaponDef skin);

        /// <summary>Unlocks a skin directly (guild rank reward) and equips it. False if not a skin or already owned.</summary>
        bool GrantSkin(WeaponDef skin);

        /// <summary>Raises the class level by one (currency handled by the caller). False at cap.</summary>
        bool TryIncrementClassLevel(WeaponClass cls);
    }
}
