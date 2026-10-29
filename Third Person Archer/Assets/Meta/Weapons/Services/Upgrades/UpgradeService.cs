using System;
using Meta.Economy;

namespace Meta.Weapons
{
    /// <summary>
    /// Upgrades the CLASS level of the given weapon's class. Price comes from the power tier's
    /// price profile at the level inside that tier.
    /// </summary>
    public class UpgradeService : IUpgradeService
    {
        // Ad upgrade = "top up with an ad": allowed when at most this share of the price is missing.
        private const float AdMaxShortfallFraction = 0.3f;
        private const int AdUpgradesPerClassPerDay = 1;

        public event System.Action<string, int> OnWeaponUpgraded;

        private readonly IClassProgressionService _progression;
        private readonly IWallet _wallet;
        private readonly WeaponCatalog _catalog;
        private readonly AdUpgradeQuota _adQuota;

        public UpgradeService(IClassProgressionService progression, IWallet wallet, WeaponCatalog catalog)
        {
            _progression = progression;
            _wallet      = wallet;
            _catalog     = catalog;
            _adQuota     = new AdUpgradeQuota(AdUpgradesPerClassPerDay);
        }

        public bool CanUpgrade(string weaponId)
        {
            var def = GetOwnedDef(weaponId);
            return def != null && TryGetUpgradeCost(def.Class, out _, out _);
        }

        public bool TryGetUpgradeCost(WeaponClass cls, out int cash, out int tokens)
        {
            cash = tokens = 0;

            var power = _progression.GetPowerDef(cls);
            if (power == null || power.UpgradePriceProfile == null || power.MaxUpgradeLevel <= 0) return false;
            if (_progression.IsAtLevelCap(cls)) return false;

            int levelInTier = _progression.GetClassLevel(cls) - _progression.GetTierStartLevel(power);
            power.UpgradePriceProfile.EvaluateUpgradeCost(levelInTier, power.MaxUpgradeLevel, out cash, out tokens);
            return true;
        }

        public bool CanAffordUpgrade(WeaponClass cls)
        {
            if (!TryGetUpgradeCost(cls, out var cash, out var tokens)) return false;
            return _wallet.CanAfford(CurrencyType.Cash, cash) &&
                   _wallet.CanAfford(WeaponCurrencyUtility.GetTokenCurrency(cls), tokens);
        }

        public bool TryUpgradeWithCurrencies(string weaponId)
        {
            var def = GetOwnedDef(weaponId);
            if (def == null || !CanAffordUpgrade(def.Class)) return false;

            TryGetUpgradeCost(def.Class, out var cashCost, out var tokenCost);
            _wallet.Spend(CurrencyType.Cash, cashCost);
            _wallet.Spend(WeaponCurrencyUtility.GetTokenCurrency(def.Class), tokenCost);

            return ApplyLevel(def.Class);
        }

        public bool CanUpgradeWithAd(string weaponId)
        {
            var def = GetOwnedDef(weaponId);
            return IsAdUpgradeAllowed(def, out _, out _, out _);
        }

        public bool TryUpgradeWithAd(string weaponId)
        {
            var def = GetOwnedDef(weaponId);
            if (!IsAdUpgradeAllowed(def, out var tokenCurrency, out var cashCost, out var tokenCost))
                return false;

            // Player pays what they have; the watched ad covers the shortfall.
            _wallet.Spend(CurrencyType.Cash, Math.Min(cashCost, _wallet.Get(CurrencyType.Cash)));
            _wallet.Spend(tokenCurrency, Math.Min(tokenCost, _wallet.Get(tokenCurrency)));
            _adQuota.Consume(def.Class);

            return ApplyLevel(def.Class);
        }

        private bool ApplyLevel(WeaponClass cls)
        {
            if (!_progression.TryIncrementClassLevel(cls)) return false;

            var power = _progression.GetPowerDef(cls);
            OnWeaponUpgraded?.Invoke(power != null ? power.Id : null, _progression.GetClassLevel(cls));
            return true;
        }

        private bool IsAdUpgradeAllowed(WeaponDef def,
                                        out CurrencyType tokenCurrency, out int cashCost, out int tokenCost)
        {
            tokenCurrency = default;
            cashCost = tokenCost = 0;

            if (def == null) return false;

            var power = _progression.GetPowerDef(def.Class);
            if (power?.UpgradePriceProfile == null || !power.UpgradePriceProfile.CanUpgradeWithAd) return false;
            if (!_adQuota.HasQuota(def.Class)) return false;
            if (!TryGetUpgradeCost(def.Class, out cashCost, out tokenCost)) return false;

            tokenCurrency = WeaponCurrencyUtility.GetTokenCurrency(def.Class);

            float cashShort = ShortfallFraction(cashCost, _wallet.Get(CurrencyType.Cash));
            float tokenShort = ShortfallFraction(tokenCost, _wallet.Get(tokenCurrency));

            // Only when the player is close (affordable upgrades use the regular button).
            float worst = Math.Max(cashShort, tokenShort);
            return worst > 0f && worst <= AdMaxShortfallFraction;
        }

        private static float ShortfallFraction(int cost, int owned)
        {
            if (cost <= 0 || owned >= cost) return 0f;
            return (float)(cost - owned) / cost;
        }

        // Any owned model of the class (tier or skin) upgrades the class.
        private WeaponDef GetOwnedDef(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId) || !_progression.IsOwned(weaponId)) return null;
            return _catalog.GetByIdOrNull(weaponId);
        }
    }
}
