using System.Linq;
using Meta.Economy;
using Meta.Weapons;

/// <summary>
/// Evaluates what the player's most useful next step is when they are blocked
/// on the next Campaign or Boss mission gate.
///
/// Decision priority (stops at the first applicable result):
/// 1. Affordable upgrade on the equipped weapon        → UpgradeWeapon
/// 2. Affordable purchase of a stronger in-class weapon → BuyWeapon
/// 3. Crossbow gate + Sniper is unlocked/playable      → PlaySniper
/// 4. Normal gate  + Contracts is unlocked/playable    → PlayContracts
/// 5. Everything else                                  → None
///
/// Crossbow gate + Sniper locked → None (per designer rule: no guidance when grind
/// path is unavailable for Crossbow gates).
///
/// Context access:
/// - Weapon services (catalog, repository) are read lazily via WeaponsInitializer.Instance
///   because WeaponsInitializer and MainMenuRuntime both run in Awake and their
///   execution order is not guaranteed.  Evaluate() is only called on map focus events,
///   well after all MonoBehaviour initialisation is complete.
/// - The wallet is accessed via Meta.Economy.Economy.Wallet (same pattern as UpgradeService).
/// </summary>
public sealed class NextStepRecommendationService
{
    private readonly IMissionAvailabilityService _availability;
    private readonly IMissionGateService _gate;
    private readonly IMissionContextService _context;
    private readonly IMetaProgressReadOnly _progress;
    private readonly IMissionCatalogService _catalog;
    private readonly IWeaponRequirementService _weaponRequirement;
    private readonly LoadoutSnapshot _snapshot;

    public NextStepRecommendationService(
        IMissionAvailabilityService availability,
        IMissionGateService gate,
        IMissionContextService context,
        IMetaProgressReadOnly progress,
        IMissionCatalogService catalog,
        IWeaponRequirementService weaponRequirement,
        LoadoutSnapshot snapshot)
    {
        _availability = availability;
        _gate = gate;
        _context = context;
        _progress = progress;
        _catalog = catalog;
        _weaponRequirement = weaponRequirement;
        _snapshot = snapshot;
    }

    // -------------------------------------------------------------------------
    // Public API
    // -------------------------------------------------------------------------

    public NextStepRecommendation Evaluate()
    {
        var baseCtx = _context.BuildSelectedContext();
        if (baseCtx?.Zone == null)
            return NextStepRecommendation.None;

        var zone = baseCtx.Zone;

        MissionGateResult gate;

        if (!zone.IsCampaignComplete())
        {
            // Target: next Campaign mission.
            var campaignCtx = BuildContextFor(baseCtx, MissionType.Campaign);
            if (campaignCtx == null || !campaignCtx.IsValid)
                return NextStepRecommendation.None;

            var avail = _availability.GetAvailability(campaignCtx);
            if (avail.CanPlay)
                return NextStepRecommendation.None;
            if (avail.Reason != AvailabilityBlockReason.CampaignDamageTooLow)
                return NextStepRecommendation.None;

            gate = _gate.CheckCampaignGate(campaignCtx);
        }
        else if (zone.IsBossUnlocked())
        {
            // Target: Boss mission.
            var bossCtx = BuildContextFor(baseCtx, MissionType.Boss);
            if (bossCtx == null || !bossCtx.IsValid)
                return NextStepRecommendation.None;

            var avail = _availability.GetAvailability(bossCtx);
            if (avail.CanPlay)
                return NextStepRecommendation.None;
            if (avail.Reason != AvailabilityBlockReason.BossCrossbowDamageTooLow)
                return NextStepRecommendation.None;

            gate = _gate.CheckBossGate(bossCtx);
        }
        else
        {
            // Campaign complete + zone has no boss (Zone 3), or boss not yet
            // unlocked (campaign not done).  Loop transition handles Zone 3.
            return NextStepRecommendation.None;
        }

        if (gate == null || gate.Passed)
            return NextStepRecommendation.None;

        var weaponClass    = gate.RequiredWeaponClass;
        bool isCrossbow    = weaponClass == WeaponClass.Crossbow;
        float currentDmg   = gate.CurrentDamage;
        float requiredDmg  = gate.RequiredDamage;

        // 1. Upgrade the currently equipped weapon?
        if (gate.CanUpgradeToPass &&
            _snapshot.TryGet(weaponClass, out var slot) &&
            slot?.Weapon != null)
        {
            var equipped = slot.Weapon;
            if (slot.UpgradeLevel < equipped.MaxUpgradeLevel &&
                CanAffordUpgrade(equipped, slot.UpgradeLevel))
            {
                return new NextStepRecommendation(
                    NextStepRecommendationType.UpgradeWeapon,
                    weaponClass, currentDmg, requiredDmg,
                    targetWeaponId: equipped.Id,
                    isCrossbowPath: isCrossbow);
            }
        }

        // 2. Buy a stronger weapon in the same class?
        int campaignIdx = GetCurrentCampaignIndex(baseCtx);
        var buyTarget = FindAffordableNextTier(weaponClass, requiredDmg, campaignIdx);
        if (buyTarget != null)
        {
            return new NextStepRecommendation(
                NextStepRecommendationType.BuyWeapon,
                weaponClass, currentDmg, requiredDmg,
                targetWeaponId: buyTarget.Id,
                isCrossbowPath: isCrossbow);
        }

        // 3 / 4. Guide the player to the appropriate resource grind.
        if (isCrossbow)
        {
            // Designer rule: if Sniper is locked when Crossbow gate is blocking,
            // return None rather than redirecting somewhere unhelpful.
            var sniperCtx  = BuildContextFor(baseCtx, MissionType.Sniper);
            var sniperAvail = sniperCtx != null
                ? _availability.GetAvailability(sniperCtx)
                : null;

            if (sniperAvail != null && sniperAvail.CanPlay)
            {
                return new NextStepRecommendation(
                    NextStepRecommendationType.PlaySniper,
                    weaponClass, currentDmg, requiredDmg,
                    targetMissionType: MissionType.Sniper,
                    isCrossbowPath: true);
            }

            return NextStepRecommendation.None;
        }
        else
        {
            var contractsCtx  = BuildContextFor(baseCtx, MissionType.Contracts);
            var contractsAvail = contractsCtx != null
                ? _availability.GetAvailability(contractsCtx)
                : null;

            if (contractsAvail != null && contractsAvail.CanPlay)
            {
                return new NextStepRecommendation(
                    NextStepRecommendationType.PlayContracts,
                    weaponClass, currentDmg, requiredDmg,
                    targetMissionType: MissionType.Contracts,
                    isCrossbowPath: false);
            }

            return NextStepRecommendation.None;
        }
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    // Mirrors the pattern used in RecommendedMissionHintService.BuildContext.
    private MissionContext BuildContextFor(MissionContext baseCtx, MissionType type)
    {
        var mission = _progress.GetMission(type);

        // Non-Contracts types need a concrete mission to be valid.
        if (type != MissionType.Contracts && mission == null)
            return null;

        int globalIndex   = _catalog.GetGlobalCampaignIndexOrMinusOne(mission);
        int companyLevel  = globalIndex >= 0 ? globalIndex + 1 : -1;

        // Null mission guard: GetRequiredWeaponClass may throw on null.
        var weaponClass = mission != null
            ? _weaponRequirement.GetRequiredWeaponClass(mission, baseCtx.LoopIndex)
            : default;

        return new MissionContext(
            baseCtx.Zone, baseCtx.ZoneIndex,
            type, mission,
            globalIndex, companyLevel,
            baseCtx.LoopIndex, baseCtx.BalanceLoopIndex,
            weaponClass);
    }

    // Returns the current global campaign index so we can filter weapon unlock levels.
    private int GetCurrentCampaignIndex(MissionContext baseCtx)
    {
        if (baseCtx.GlobalCampaignIndex >= 0)
            return baseCtx.GlobalCampaignIndex;

        // baseCtx is not a Campaign context (e.g. Contracts/Boss selected).
        // Derive from the next campaign mission instead.
        var campaignMission = _progress.GetMission(MissionType.Campaign);
        return _catalog.GetGlobalCampaignIndexOrMinusOne(campaignMission);
    }

    // True if the player can pay for the next upgrade level right now.
    // Note: UpgradeService.CanUpgrade only checks remaining levels, not wallet.
    private bool CanAffordUpgrade(WeaponDef def, int currentLevel)
    {
        var profile = def.UpgradePriceProfile;
        if (profile == null) return false;

        profile.EvaluateUpgradeCost(currentLevel, def.MaxUpgradeLevel,
            out int cash, out int tokens);

        var wallet = Meta.Economy.Economy.Wallet;
        if (wallet == null) return false;

        var tokenCurrency = WeaponCurrencyUtility.GetTokenCurrency(def.Class);
        return wallet.CanAfford(CurrencyType.Cash, cash) &&
               wallet.CanAfford(tokenCurrency, tokens);
    }

    // Returns the first unowned, unlocked, affordable weapon in the class whose
    // BASE damage already meets the gate requirement (player can proceed immediately
    // after buying without any additional upgrades).
    private WeaponDef FindAffordableNextTier(WeaponClass cls, float requiredDamage, int campaignIndex)
    {
        var initializer = Meta.Weapons.WeaponsInitializer.Instance;
        if (initializer == null) return null;

        var catalog = initializer.WeaponCatalog;
        var repo    = initializer.WeaponRepository;
        if (catalog == null || repo == null) return null;

        var wallet = Meta.Economy.Economy.Wallet;
        if (wallet == null) return null;

        var state         = repo.Load();
        var tokenCurrency = WeaponCurrencyUtility.GetTokenCurrency(cls);

        foreach (var def in catalog.GetByClass(cls))
        {
            // Base damage must already pass the gate on purchase day-one.
            if (def.BaseStats.Damage < requiredDamage) continue;

            // Must be unlocked by the player's current campaign progress.
            if (def.UnlockAfterCampaignLevel > campaignIndex) continue;

            // Skip weapons the player already owns.
            if (state.Weapons.Any(w => w.WeaponId == def.Id && w.Owned)) continue;

            // Both cash and class-token must be affordable.
            if (wallet.CanAfford(CurrencyType.Cash, def.PurchaseCash) &&
                wallet.CanAfford(tokenCurrency, def.PurchaseTokens))
            {
                return def;
            }
        }

        return null;
    }
}
