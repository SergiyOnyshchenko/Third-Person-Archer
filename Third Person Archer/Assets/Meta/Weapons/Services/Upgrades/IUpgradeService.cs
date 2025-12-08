using System;

namespace Meta.Weapons
{
    public enum UpgradePayment
    {
        Currencies, // Money + class-specific token
        Ad          // Ad-based free upgrade
    }

    public interface IUpgradeService
    {
        /// <summary>Raised after a successful upgrade.</summary>
        event Action<string, int> OnWeaponUpgraded; // (weaponId, newLevel)

        bool CanUpgrade(string weaponId);
        bool TryUpgradeWithCurrencies(string weaponId); // Cash + class token
        bool TryUpgradeWithAd(string weaponId);         // no wallet cost
    }
}