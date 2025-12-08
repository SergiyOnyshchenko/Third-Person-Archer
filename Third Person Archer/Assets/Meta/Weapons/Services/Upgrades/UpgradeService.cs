using System.Linq;
using Meta.Economy;

namespace Meta.Weapons
{
    public class UpgradeService : IUpgradeService
    {
        public event System.Action<string, int> OnWeaponUpgraded;

        private readonly IWeaponRepository _weaponRepo;
        private readonly IWallet _wallet;
        private readonly WeaponDef[] _allDefs;

        public UpgradeService(IWeaponRepository weaponRepo,
                              IWallet wallet,
                              WeaponDef[] allDefs)
        {
            _weaponRepo = weaponRepo;
            _wallet     = wallet;
            _allDefs    = allDefs;
        }

        public bool CanUpgrade(string weaponId)
        {
            var (def, inst, _) = GetDefAndInstance(weaponId);
            if (def == null || inst == null) return false;

            if (def.UpgradePriceProfile == null) return false;
            return inst.UpgradeLevel < def.MaxUpgradeLevel;
        }

        public bool TryUpgradeWithCurrencies(string weaponId)
        {
            var (def, inst, state) = GetDefAndInstance(weaponId);
            if (def == null || inst == null) return false;

            var profile = def.UpgradePriceProfile;
            if (profile == null) return false;

            if (inst.UpgradeLevel >= def.MaxUpgradeLevel)
                return false;

            profile.EvaluateUpgradeCost(inst.UpgradeLevel, def.MaxUpgradeLevel,
                                        out var cashCost, out var tokenCost);

            var tokenCurrency = WeaponCurrencyUtility.GetTokenCurrency(def.Class);

            if (!_wallet.CanAfford(CurrencyType.Cash, cashCost)) return false;
            if (!_wallet.CanAfford(tokenCurrency, tokenCost)) return false;

            _wallet.Spend(CurrencyType.Cash, cashCost);
            _wallet.Spend(tokenCurrency, tokenCost);

            inst.UpgradeLevel++;
            _weaponRepo.Save(state);
            OnWeaponUpgraded?.Invoke(weaponId, inst.UpgradeLevel);
            return true;
        }

        public bool TryUpgradeWithAd(string weaponId)
        {
            var (def, inst, state) = GetDefAndInstance(weaponId);
            if (def == null || inst == null) return false;

            var profile = def.UpgradePriceProfile;
            if (profile == null || !profile.CanUpgradeWithAd)
                return false;

            if (inst.UpgradeLevel >= def.MaxUpgradeLevel)
                return false;

            // No wallet checks: ad already rewarded externally.
            inst.UpgradeLevel++;
            _weaponRepo.Save(state);
            OnWeaponUpgraded?.Invoke(weaponId, inst.UpgradeLevel);
            return true;
        }

        private (WeaponDef def, WeaponInstance inst, WeaponsState state) GetDefAndInstance(string weaponId)
        {
            var state = _weaponRepo.Load();
            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == weaponId && w.Owned);
            if (inst == null) return (null, null, state);

            var def = _allDefs.FirstOrDefault(d => d.Id == weaponId);
            return (def, inst, state);
        }
    }
}