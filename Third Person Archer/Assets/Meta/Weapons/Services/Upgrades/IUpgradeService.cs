using System;

namespace Meta.Weapons
{
    public enum UpgradePayment
    {
        Currencies, // Money + class-specific token
        Ad          // Ad-based free upgrade
    }

    /// <summary>
    /// Upgrades are per class: any owned model id of a class raises that class level.
    /// </summary>
    public interface IUpgradeService
    {
        /// <summary>Raised after a successful upgrade.</summary>
        event Action<string, int> OnWeaponUpgraded; // (power weaponId, new class level)

        bool CanUpgrade(string weaponId);               // below the level cap (wallet not checked)
        bool TryGetUpgradeCost(WeaponClass cls, out int cash, out int tokens); // false at cap
        bool CanAffordUpgrade(WeaponClass cls);
        bool TryUpgradeWithCurrencies(string weaponId); // Cash + class token
        bool CanUpgradeWithAd(string weaponId);         // short by <=30% of the price, daily quota left
        bool TryUpgradeWithAd(string weaponId);         // call after the ad rewarded; spends what the player has
    }
}
