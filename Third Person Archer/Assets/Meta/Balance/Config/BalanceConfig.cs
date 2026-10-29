using UnityEngine;
using Meta.Weapons;

[CreateAssetMenu(fileName = "BalanceConfig", menuName = "GameMeta/BalanceConfig")]
public partial class BalanceConfig : ScriptableObject
{
    [Header("Modules (Single ScriptableObject)")]
    [SerializeField] private EnemyModule _enemies = new();
    [SerializeField] private GateModule _gates = new();
    [SerializeField] private RewardModule _rewards = new();
    [SerializeField] private WeaponClassPerformanceModule _weaponPerformance = new();
    [SerializeField] private RoundingModule _rounding = new();
    [UnityEngine.Serialization.FormerlySerializedAs("_guild")]
    [SerializeField] private ReplayModule _replay = new();
    [SerializeField] private HeroicModule _heroic = new();

    [Header("Weapon Tiers (damage ranges for tier gates)")]
    [SerializeField] private WeaponCatalog _weaponCatalog;

    public ReplayModule Replay => _replay;
    public HeroicModule Heroic => _heroic;
    public WeaponCatalog WeaponCatalog => _weaponCatalog;

    #region Enemy & Difficulty API

    public EnemyModule.EnemyTypeProfile GetEnemyProfile(EnemyArchetype archetype) =>
        _enemies.GetEnemyProfile(archetype);

    /// <summary>
    /// Unified difficulty scalar.
    /// progressionIndex meaning:
    /// - Campaign: globalCampaignIndex
    /// - Contracts: contractsCompletedIndex
    /// - Sniper: sniperCompletedIndex
    /// - Boss: excluded (returns 1)
    /// </summary>
    public float GetDifficultyScalar(
        MissionType type,
        int progressionIndex,
        WeaponClass weaponClass)
    {
        progressionIndex = Mathf.Max(0, progressionIndex);

        switch (type)
        {
            case MissionType.Campaign:
                return _enemies.GetCampaignDifficultyScalar(progressionIndex);

            case MissionType.Contracts:
                return _enemies.GetContractsDifficultyScalar(progressionIndex) * _enemies.GetWeaponClassDifficultyMultiplier(weaponClass);

            case MissionType.Sniper:
                return _enemies.GetSniperDifficultyScalar(progressionIndex) * _enemies.GetWeaponClassDifficultyMultiplier(weaponClass);

            case MissionType.Boss:
                // Boss excluded from formula balancing per Archer.
                return 1f;

            default:
                return 1f;
        }
    }

    private float GetEnemyHpFromGateFactor(MissionType type)
    {
        switch (type)
        {
            case MissionType.Campaign: return _enemies.CampaignHpFromGate;
            case MissionType.Contracts: return _enemies.ContractHpFromGate;
            case MissionType.Sniper: return _enemies.SniperHpFromGate;
            default: return _enemies.CampaignHpFromGate;
        }
    }

    private float GetEnemyDamageFromGateFactor(MissionType type)
    {
        switch (type)
        {
            case MissionType.Campaign: return _enemies.CampaignDamageFromGate;
            case MissionType.Contracts: return _enemies.ContractDamageFromGate;
            case MissionType.Sniper: return _enemies.SniperDamageFromGate;
            default: return _enemies.CampaignDamageFromGate;
        }
    }

    /// <summary>
    /// Formula-driven enemy stats (no EnemyStatsConfig).
    /// IMPORTANT: Contracts/Sniper difficulty depends on external counters.
    /// Pass counters from GameplayRuntime (MetaLoopProgressData).
    /// </summary>
    public EnemyModule.EnemyStats GetEnemyStats(
        MissionContext ctx,
        EnemyArchetype archetype,
        int contractsCompletedIndex,
        int sniperCompletedIndex,
        bool isMultiplayer)
    {
        if (ctx == null || !ctx.IsValid)
            return new EnemyModule.EnemyStats(1, 0);

        if (ctx.SelectedType == MissionType.Boss)
            return new EnemyModule.EnemyStats(1, 0);

        MissionType effectiveType = ctx.BalanceMode;

        int effectiveContractsCompletedIndex =
            ctx.ContractsCompletedIndexOverride >= 0 ? ctx.ContractsCompletedIndexOverride : contractsCompletedIndex;

        int gateIndex =
            ctx.GlobalCampaignIndex >= 0 ? ctx.GlobalCampaignIndex :
            ctx.CompanyLevel > 0 ? (ctx.CompanyLevel - 1) :
            0;

        // Campaign and replays use the tier R below; this legacy value only feeds enemy damage.
        float gateDamage = GetRequiredDamageForCampaign(ctx.RequiredWeaponClass, gateIndex);

        int progressionIndex = effectiveType switch
        {
            MissionType.Campaign => Mathf.Max(0, ctx.GlobalCampaignIndex),
            MissionType.Contracts => Mathf.Max(0, effectiveContractsCompletedIndex),
            MissionType.Sniper => Mathf.Max(0, sniperCompletedIndex),
            _ => 0
        };

        float difficulty = GetDifficultyScalar(effectiveType, progressionIndex, ctx.RequiredWeaponClass);

        var profile = GetEnemyProfile(archetype);
        float typeHp = profile != null ? profile.HpMultiplier : 1f;
        float typeDmg = profile != null ? profile.DamageMultiplier : 1f;

        float hpFromGate = GetEnemyHpFromGateFactor(effectiveType);
        float dmgFromGate = GetEnemyDamageFromGateFactor(effectiveType);

        float mpHp = isMultiplayer ? _enemies.MultiplayerHpMultiplier : 1f;
        float mpDmg = isMultiplayer ? _enemies.MultiplayerDamageMultiplier : 1f;

        int dmg = Mathf.Max(0, Mathf.RoundToInt(gateDamage * dmgFromGate * difficulty * typeDmg * mpDmg));

        // Replay of a cleared level: no upward auto-scaling. HP = R of the played class for the replayed level,
        // capped so the player's weapon never needs more than MaxBodyShots body shots.
        if (ctx.SelectedType == MissionType.Contracts)
        {
            int recommended = GetContractRecommendedDamage(ctx);
            int contractHp = Mathf.Max(1, Mathf.FloorToInt(recommended * Mathf.Min(1f, typeHp) * mpHp));
            if (ctx.ContractPlayerDamage > 0)
                contractHp = Mathf.Min(contractHp, ctx.ContractPlayerDamage * _replay.MaxBodyShots);

            int contractDmg = Mathf.Max(0, Mathf.RoundToInt(dmg * _replay.EnemyDamageMultiplier));
            return new EnemyModule.EnemyStats(contractHp, contractDmg);
        }

        // Campaign: 1–2 shot rule. HP = R * type multiplier (no difficulty scaling, no sponge factor);
        // difficulty comes from enemy damage, count and placement instead.
        if (effectiveType == MissionType.Campaign)
        {
            int recommended = GetCampaignRecommendedDamage(ctx);
            int campaignHp = Mathf.Max(1, Mathf.FloorToInt(recommended * Mathf.Min(1f, typeHp) * mpHp));
            return new EnemyModule.EnemyStats(campaignHp, dmg);
        }

        int hp = Mathf.Max(1, Mathf.RoundToInt(gateDamage * hpFromGate * difficulty * typeHp * mpHp));

        // Health clamp: prevent bullet-sponge enemies (Contracts, if enabled in the profile).
        // maxAllowedHp = gateDamage * maxShots.
        if (_enemies.ShouldApplyHealthClamp(ctx.RequiredWeaponClass, effectiveType))
        {
            int maxShots = _enemies.GetMaxShotsToKill(ctx.RequiredWeaponClass, archetype);
            if (maxShots > 0)
            {
                int maxAllowedHp = Mathf.Max(1, Mathf.RoundToInt(gateDamage * maxShots));
                hp = Mathf.Min(hp, maxAllowedHp);
            }
        }

        return new EnemyModule.EnemyStats(hp, dmg);
    }

    #endregion

    #region Gate API

    /// <summary>Legacy formula (editor generator, debug easy mode, fallback when a tier is missing).</summary>
    public float GetRequiredDamageForCampaign(WeaponClass weaponClass, int globalIndex)
    {
        return Mathf.Max(0f, _gates.GetRequiredDamageForCampaign(weaponClass, globalIndex));
    }

    /// <summary>
    /// Recommended power R for a Campaign mission: at this damage every regular enemy dies
    /// in one body shot. R = damage of the class's zone tier at the mission's position among
    /// same-class missions of the zone. Integer because in-game damage is integer.
    /// </summary>
    public int GetCampaignRecommendedDamage(MissionContext ctx)
    {
        if (ctx == null || !ctx.IsValid) return 1;

        if (ctx.IsHeroic)
        {
            var heroicDef = FindTierDef(ctx.RequiredWeaponClass, _heroic.GetTier(ctx.RequiredWeaponClass));
            if (heroicDef != null)
                return Mathf.Max(1, Mathf.FloorToInt(heroicDef.GetDamageAt01(ctx.HeroicPosition01)));
        }

        float position = GetSegmentPosition01(ctx.Zone, MissionType.Campaign, ctx.Mission, ctx.RequiredWeaponClass);
        float value = GetTierGateDamage(ctx.RequiredWeaponClass, ctx.ZoneIndex, position,
            fallbackGlobalIndex: ctx.GlobalCampaignIndex);

        return Mathf.Max(1, Mathf.FloorToInt(value));
    }

    /// <summary>
    /// R of a replayed level for the class the player brought: tier of that class in the level's zone
    /// (at least T1 — e.g. spear on a Zone 1 level) at the level's position among the class's missions.
    /// After the campaign (Heroic era) it is the Heroic tier at the whole-campaign position, like Heroic levels.
    /// </summary>
    public int GetContractRecommendedDamage(MissionContext ctx)
    {
        if (ctx == null || !ctx.IsValid) return 1;

        var cls = ctx.RequiredWeaponClass;
        float position;
        WeaponDef tierDef;

        if (ctx.IsHeroicEra && MissionProgressData.Active != null)
        {
            position = MissionProgressData.Active.GetHeroicPosition01(ctx.Mission, cls);
            tierDef = FindTierDef(cls, _heroic.GetTier(cls));
        }
        else
        {
            position = GetSegmentPosition01(ctx.Zone, MissionType.Campaign, ctx.Mission, cls);
            tierDef = FindTierDef(cls, Mathf.Max(1, _gates.GetTierForZone(cls, ctx.ZoneIndex)));
        }

        float value = tierDef != null
            ? tierDef.GetDamageAt01(position)
            : GetRequiredDamageForCampaign(cls, Mathf.Max(0, ctx.CompanyLevel - 1));

        return Mathf.Max(1, Mathf.FloorToInt(value));
    }

    /// <summary>
    /// Minimum damage to start a Campaign mission (below = Red, blocked).
    /// ceil(R * fraction) with fraction >= 0.5 → two body shots always kill (2 * min >= R >= enemy HP).
    /// </summary>
    public int GetCampaignMinDamage(MissionContext ctx)
    {
        int recommended = GetCampaignRecommendedDamage(ctx);
        return Mathf.Max(1, Mathf.CeilToInt(recommended * _gates.CampaignMinDamageFraction));
    }

    /// <summary>
    /// Hard Crossbow gate of a Boss: zone's Crossbow tier at BossTierPosition.
    /// Does NOT affect boss HP/damage gameplay scaling — purely a meta access gate.
    /// </summary>
    public float GetBossGateDamage(MissionContext ctx)
    {
        if (ctx == null) return 0f;

        return Mathf.Floor(GetTierGateDamage(WeaponClass.Crossbow, ctx.ZoneIndex, _gates.BossTierPosition,
            fallbackGlobalIndex: Mathf.Max(0, ctx.CompanyLevel - 1)));
    }

    /// <summary>Damage of the class's zone tier at position01 of its level range.</summary>
    public float GetTierGateDamage(WeaponClass weaponClass, int zoneIndex, float position01, int fallbackGlobalIndex = 0)
    {
        var tierDef = FindTierDef(weaponClass, _gates.GetTierForZone(weaponClass, zoneIndex));
        if (tierDef == null)
            return GetRequiredDamageForCampaign(weaponClass, fallbackGlobalIndex);

        return Mathf.Max(0f, tierDef.GetDamageAt01(position01));
    }

    private WeaponDef FindTierDef(WeaponClass weaponClass, int tier)
    {
        if (_weaponCatalog == null || tier <= 0) return null;

        foreach (var def in _weaponCatalog.GetByClass(weaponClass))
            if (def != null && !def.IsSkin && def.Tier == tier)
                return def;

        return null;
    }

    /// <summary>
    /// k / n: k = same-class missions before this one in the zone segment, n = same-class missions in it.
    /// </summary>
    private static float GetSegmentPosition01(ZoneData zone, MissionType type, MissionData mission, WeaponClass weaponClass)
    {
        var missions = zone != null ? zone.GetSegmentByType(type)?.Missions : null;
        if (missions == null || mission == null) return 0f;

        int before = 0, total = 0;
        bool found = false;

        foreach (var m in missions)
        {
            if (m == null) continue;
            if (m == mission) found = true;

            var cls = m.UseFixedWeaponClass ? m.FixedWeaponClass : m.BaseWeaponClass;
            if (type != MissionType.Campaign || cls == weaponClass)
            {
                total++;
                if (!found) before++;
            }
        }

        return found && total > 0 ? (float)before / total : 0f;
    }

    #endregion

    #region Rewards API

    public MissionRewardResult GetMissionReward(
        MissionType missionType,
        int rewardIndex,
        WeaponClass missionWeaponClass)
    {
        int weaponClassCount = System.Enum.GetValues(typeof(WeaponClass)).Length;
        return _rewards.GetMissionReward(
            missionType,
            rewardIndex,
            missionWeaponClass,
            weaponClassCount,
            _rounding);
    }

    /// <summary>
    /// Contract reward: profile by the level's zone (gold + essence of the played class), times the
    /// order / daily bonus multiplier. Heroic era: the profile index is shifted (enemies use the Heroic tier).
    /// </summary>
    public MissionRewardResult GetContractReward(int levelZoneIndex, WeaponClass playedClass, float multiplier)
    {
        bool heroic = MissionProgressData.Active != null && MissionProgressData.Active.IsHeroicUnlocked();
        int index = Mathf.Max(0, levelZoneIndex) + (heroic ? _heroic.ContractZoneOffset : 0);
        var raw = GetMissionReward(MissionType.Contracts, index, playedClass);
        if (Mathf.Approximately(multiplier, 1f))
            return raw;

        var tokens = raw.TokensPerType;
        for (int i = 0; i < tokens.Length; i++)
            tokens[i] = Mathf.RoundToInt(tokens[i] * multiplier);

        return new MissionRewardResult(_rounding.RoundMoneyToNiceInt(raw.Cash * multiplier), tokens);
    }

    #endregion

    #region Generator helpers

    public float GetWeaponClassDamageMultiplier(WeaponClass weaponClass) => _weaponPerformance.GetDamageMultiplier(weaponClass);
    public RoundingModule Rounding => _rounding;

    #endregion
}
