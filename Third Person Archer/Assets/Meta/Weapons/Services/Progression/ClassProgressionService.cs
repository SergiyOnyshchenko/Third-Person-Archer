using System;
using System.Collections.Generic;
using System.Linq;
using Meta.Economy;

namespace Meta.Weapons
{
    public sealed class ClassProgressionService : IClassProgressionService
    {
        private const int CurrentProgressionVersion = 1;

        public event Action<WeaponClass> OnProgressChanged;

        private readonly IWeaponRepository _repo;
        private readonly IWallet _wallet;
        private readonly IEquipmentService _equipment;
        private readonly IStatsService _stats;
        private readonly WeaponCatalog _catalog;

        public ClassProgressionService(IWeaponRepository repo, IWallet wallet, IEquipmentService equipment,
                                       IStatsService stats, WeaponCatalog catalog)
        {
            _repo = repo;
            _wallet = wallet;
            _equipment = equipment;
            _stats = stats;
            _catalog = catalog;
        }

        // ── Levels ───────────────────────────────────────────────────────────

        public int GetClassLevel(WeaponClass cls) => GetClassLevel(_repo.Load(), cls);

        public int GetLevelCap(WeaponClass cls)
        {
            var power = GetPowerDef(cls);
            return power != null ? GetTierStartLevel(power) + power.MaxUpgradeLevel : 0;
        }

        public bool IsAtLevelCap(WeaponClass cls) => GetClassLevel(cls) >= GetLevelCap(cls);

        public bool TryIncrementClassLevel(WeaponClass cls)
        {
            var state = _repo.Load();
            int level = GetClassLevel(state, cls);
            if (level >= GetLevelCap(state, cls)) return false;

            SetClassLevel(state, cls, level + 1);
            _repo.Save(state);
            OnProgressChanged?.Invoke(cls);
            return true;
        }

        // ── Tiers ────────────────────────────────────────────────────────────

        public WeaponDef GetPowerDef(WeaponClass cls) => GetPowerDef(_repo.Load(), cls);

        public WeaponDef GetNextTierDef(WeaponClass cls)
        {
            var power = GetPowerDef(cls);
            int powerTier = power != null ? power.Tier : 0;
            return GetTiers(cls).FirstOrDefault(d => d.Tier > powerTier);
        }

        public WeaponDef GetTierDef(WeaponClass cls, int tier) =>
            GetTiers(cls).FirstOrDefault(d => d.Tier == tier);

        public int GetTierStartLevel(WeaponDef tierDef)
        {
            if (tierDef == null || tierDef.IsSkin) return 0;
            return GetTiers(tierDef.Class).Where(d => d.Tier < tierDef.Tier).Sum(d => d.MaxUpgradeLevel);
        }

        public WeaponStats GetPowerStats(WeaponClass cls)
        {
            var state = _repo.Load();
            var power = GetPowerDef(state, cls);
            return power != null ? GetTierStats(power, GetClassLevel(state, cls)) : WeaponStats.Zero;
        }

        public WeaponStats GetTierStats(WeaponDef tierDef, int classLevel)
        {
            if (tierDef == null) return WeaponStats.Zero;
            if (tierDef.IsSkin) return GetPowerStats(tierDef.Class);

            int levelInTier = Math.Clamp(classLevel - GetTierStartLevel(tierDef), 0, tierDef.MaxUpgradeLevel);
            return _stats.Compute(tierDef, levelInTier);
        }

        public bool IsOwned(string weaponId) => IsOwned(_repo.Load(), weaponId);

        public TierPurchaseBlock GetTierPurchaseBlock(WeaponDef def, int companyLevel)
        {
            if (def == null || def.IsSkin) return TierPurchaseBlock.NotATier;

            var state = _repo.Load();
            if (IsOwned(state, def.Id)) return TierPurchaseBlock.AlreadyOwned;
            if (companyLevel < def.UnlockAfterCampaignLevel) return TierPurchaseBlock.CampaignLocked;

            // Tiers are bought in order; the previous one must be maxed so no level is ever wasted.
            var previous = GetTiers(def.Class).LastOrDefault(d => d.Tier < def.Tier);
            if (previous != null)
            {
                if (!IsOwned(state, previous.Id)) return TierPurchaseBlock.PreviousTierMissing;
                if (GetClassLevel(state, def.Class) < GetTierStartLevel(previous) + previous.MaxUpgradeLevel)
                    return TierPurchaseBlock.PreviousTierNotMaxed;
            }

            var token = WeaponCurrencyUtility.GetTokenCurrency(def.Class);
            if (!_wallet.CanAfford(CurrencyType.Cash, def.PurchaseCash) || !_wallet.CanAfford(token, def.PurchaseTokens))
                return TierPurchaseBlock.NotEnoughCurrency;

            return TierPurchaseBlock.None;
        }

        public bool TryBuyTier(WeaponDef def, int companyLevel)
        {
            if (GetTierPurchaseBlock(def, companyLevel) != TierPurchaseBlock.None) return false;

            _wallet.Spend(CurrencyType.Cash, def.PurchaseCash);
            _wallet.Spend(WeaponCurrencyUtility.GetTokenCurrency(def.Class), def.PurchaseTokens);

            var state = _repo.Load();
            SetOwned(state, def.Id);
            _repo.Save(state);

            // The new tier is the strongest model: show it right away.
            _equipment.Equip(def.Class, def.Id);
            OnProgressChanged?.Invoke(def.Class);
            return true;
        }

        // ── Skins ────────────────────────────────────────────────────────────

        public int GetSkinAdsWatched(WeaponDef skin)
        {
            if (skin == null) return 0;
            var entry = _repo.Load().SkinAds.FirstOrDefault(s => s.WeaponId == skin.Id);
            return entry != null ? entry.AdsWatched : 0;
        }

        public bool RegisterSkinAd(WeaponDef skin)
        {
            if (skin == null || !skin.IsSkin) return false;

            var state = _repo.Load();
            if (IsOwned(state, skin.Id)) return true;

            var entry = state.SkinAds.FirstOrDefault(s => s.WeaponId == skin.Id);
            if (entry == null)
            {
                entry = new SkinAdProgress { WeaponId = skin.Id };
                state.SkinAds.Add(entry);
            }

            entry.AdsWatched++;
            bool unlocked = entry.AdsWatched >= skin.SkinAdsRequired;
            if (unlocked)
                SetOwned(state, skin.Id);

            _repo.Save(state);

            if (unlocked)
                _equipment.Equip(skin.Class, skin.Id);

            OnProgressChanged?.Invoke(skin.Class);
            return unlocked;
        }

        public bool GrantSkin(WeaponDef skin)
        {
            if (skin == null || !skin.IsSkin) return false;

            var state = _repo.Load();
            if (IsOwned(state, skin.Id)) return false;

            SetOwned(state, skin.Id);
            _repo.Save(state);
            _equipment.Equip(skin.Class, skin.Id);

            OnProgressChanged?.Invoke(skin.Class);
            return true;
        }

        // ── Save migration ───────────────────────────────────────────────────

        /// <summary>
        /// Old saves kept a level per model. New class level = start of the best owned tier + that model's level.
        /// Call after default weapons are granted.
        /// </summary>
        public void MigrateIfNeeded()
        {
            var state = _repo.Load();
            if (state.ProgressionVersion >= CurrentProgressionVersion) return;

            foreach (WeaponClass cls in Enum.GetValues(typeof(WeaponClass)))
            {
                var power = GetPowerDef(state, cls);
                if (power == null) continue;

                var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == power.Id && w.Owned);
                int level = GetTierStartLevel(power) + (inst != null ? inst.UpgradeLevel : 0);
                SetClassLevel(state, cls, Math.Min(level, GetLevelCap(state, cls)));
            }

            state.ProgressionVersion = CurrentProgressionVersion;
            _repo.Save(state);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private IEnumerable<WeaponDef> GetTiers(WeaponClass cls) =>
            _catalog.GetByClass(cls).Where(d => d != null && !d.IsSkin).OrderBy(d => d.Tier);

        private WeaponDef GetPowerDef(WeaponsState state, WeaponClass cls) =>
            GetTiers(cls).LastOrDefault(d => IsOwned(state, d.Id));

        private int GetLevelCap(WeaponsState state, WeaponClass cls)
        {
            var power = GetPowerDef(state, cls);
            return power != null ? GetTierStartLevel(power) + power.MaxUpgradeLevel : 0;
        }

        private static int GetClassLevel(WeaponsState state, WeaponClass cls)
        {
            var entry = state.ClassLevels.FirstOrDefault(c => c.Class == cls);
            return entry != null ? entry.Level : 0;
        }

        private static void SetClassLevel(WeaponsState state, WeaponClass cls, int level)
        {
            var entry = state.ClassLevels.FirstOrDefault(c => c.Class == cls);
            if (entry == null)
            {
                entry = new WeaponClassLevel { Class = cls };
                state.ClassLevels.Add(entry);
            }
            entry.Level = Math.Max(0, level);
        }

        private static bool IsOwned(WeaponsState state, string weaponId) =>
            state.Weapons.Any(w => w.WeaponId == weaponId && w.Owned);

        private static void SetOwned(WeaponsState state, string weaponId)
        {
            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == weaponId);
            if (inst == null)
                state.Weapons.Add(new WeaponInstance { WeaponId = weaponId, Owned = true });
            else
                inst.Owned = true;
        }
    }
}
