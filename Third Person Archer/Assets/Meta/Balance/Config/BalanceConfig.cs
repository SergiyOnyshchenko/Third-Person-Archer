using UnityEngine;
using Meta.Weapons;

[CreateAssetMenu(fileName = "BalanceConfig", menuName = "GameMeta/BalanceConfig")]
public partial class BalanceConfig : ScriptableObject
{
    [Header("Modules (Single ScriptableObject)")]
    [SerializeField] private LoopModule _loops = new();
    [SerializeField] private EnemyModule _enemies = new();
    [SerializeField] private GateModule _gates = new();
    [SerializeField] private RewardModule _rewards = new();
    [SerializeField] private WeaponClassPerformanceModule _weaponPerformance = new();
    [SerializeField] private RoundingModule _rounding = new();

    #region Loop API

    public int ClampLoopIndex(int loopIndex) => _loops.ClampLoopIndex(loopIndex);
    public LoopMultipliers GetLoopMultipliers(int loopIndex) => _loops.GetLoopMultipliers(loopIndex);

    #endregion

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
        WeaponClass weaponClass,
        int balanceLoopIndex)
    {
        balanceLoopIndex = ClampLoopIndex(balanceLoopIndex);
        progressionIndex = Mathf.Max(0, progressionIndex);

        switch (type)
        {
            case MissionType.Campaign:
                return GetCampaignDifficultyScalar(progressionIndex, balanceLoopIndex);

            case MissionType.Contracts:
                return GetContractsDifficultyScalar(progressionIndex, weaponClass, balanceLoopIndex);

            case MissionType.Sniper:
                return GetSniperDifficultyScalar(progressionIndex, weaponClass, balanceLoopIndex);

            case MissionType.Boss:
                // Boss excluded from formula balancing per Archer.
                return 1f;

            default:
                return 1f;
        }
    }

    private float GetCampaignDifficultyScalar(int globalCampaignIndex, int loopIndex)
    {
        float baseValue = _enemies.GetCampaignDifficultyScalar(globalCampaignIndex);
        return ApplyLoopDifficulty(MissionType.Campaign, baseValue, loopIndex);
    }

    private float GetContractsDifficultyScalar(int contractsCompleted, WeaponClass requiredClass, int loopIndex)
    {
        float baseValue = _enemies.GetContractsDifficultyScalar(contractsCompleted);
        float classMult = _enemies.GetWeaponClassDifficultyMultiplier(requiredClass);

        float raw = baseValue * classMult;
        return ApplyLoopDifficulty(MissionType.Contracts, raw, loopIndex);
    }

    private float GetSniperDifficultyScalar(int sniperCompleted, WeaponClass requiredClass, int loopIndex)
    {
        float baseValue = _enemies.GetSniperDifficultyScalar(sniperCompleted);
        float classMult = _enemies.GetWeaponClassDifficultyMultiplier(requiredClass);

        float raw = baseValue * classMult;
        return ApplyLoopDifficulty(MissionType.Sniper, raw, loopIndex);
    }

    private float ApplyLoopDifficulty(MissionType type, float raw, int loopIndex)
    {
        var m = GetLoopMultipliers(loopIndex);

        switch (type)
        {
            case MissionType.Campaign: return raw * m.CampaignDifficulty;
            case MissionType.Contracts: return raw * m.ContractDifficulty;
            case MissionType.Sniper: return raw * m.SniperDifficulty;
            default: return raw;
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

        int loopIndex = ClampLoopIndex(ctx.BalanceLoopIndex);

        int gateIndex =
            ctx.GlobalCampaignIndex >= 0 ? ctx.GlobalCampaignIndex :
            ctx.CompanyLevel > 0 ? (ctx.CompanyLevel - 1) :
            0;

        float gateDamage = GetRequiredDamageForCampaign(ctx.RequiredWeaponClass, gateIndex, loopIndex);

        int progressionIndex = effectiveType switch
        {
            MissionType.Campaign => Mathf.Max(0, ctx.GlobalCampaignIndex),
            MissionType.Contracts => Mathf.Max(0, effectiveContractsCompletedIndex),
            MissionType.Sniper => Mathf.Max(0, sniperCompletedIndex),
            _ => 0
        };

        float difficulty = GetDifficultyScalar(effectiveType, progressionIndex, ctx.RequiredWeaponClass, loopIndex);

        var profile = GetEnemyProfile(archetype);
        float typeHp = profile != null ? profile.HpMultiplier : 1f;
        float typeDmg = profile != null ? profile.DamageMultiplier : 1f;

        float hpFromGate = GetEnemyHpFromGateFactor(effectiveType);
        float dmgFromGate = GetEnemyDamageFromGateFactor(effectiveType);

        float mpHp = isMultiplayer ? _enemies.MultiplayerHpMultiplier : 1f;
        float mpDmg = isMultiplayer ? _enemies.MultiplayerDamageMultiplier : 1f;

        int hp = Mathf.Max(1, Mathf.RoundToInt(gateDamage * hpFromGate * difficulty * typeHp * mpHp));
        int dmg = Mathf.Max(0, Mathf.RoundToInt(gateDamage * dmgFromGate * difficulty * typeDmg * mpDmg));

        // Campaign health clamp: prevent bullet-sponge enemies.
        // maxAllowedHp = gateDamage * maxShots — scales with loop since gateDamage includes loop GateDamage multiplier.
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

    public float GetRequiredDamageForCampaign(WeaponClass weaponClass, int globalIndex, int loopIndex)
    {
        float value = _gates.GetRequiredDamageForCampaign(weaponClass, globalIndex);
        value *= GetLoopMultipliers(loopIndex).GateDamage;
        return Mathf.Max(0f, value);
    }

    /// <summary>
    /// Required Crossbow damage to access Sniper missions at the given tier.
    /// sniperCompletedIndex = total Sniper missions completed so far.
    /// GateDamage loop multiplier is applied so higher loops are harder.
    /// </summary>
    public float GetRequiredCrossbowDamageForSniper(int sniperCompletedIndex, int loopIndex)
    {
        float value = _gates.GetRequiredCrossbowDamageForSniper(sniperCompletedIndex);
        value *= GetLoopMultipliers(loopIndex).GateDamage;
        return Mathf.Max(0f, value);
    }

    /// <summary>
    /// Required Crossbow damage to access a Boss mission in the given zone.
    /// bossZoneIndex = 0-based zone index (Zone 1 = 0, Zone 2 = 1).
    /// GateDamage loop multiplier is applied so higher loops require stronger Crossbow.
    /// Does NOT affect boss HP/damage gameplay scaling — purely a meta access gate.
    /// </summary>
    public float GetRequiredCrossbowDamageForBoss(int bossZoneIndex, int loopIndex)
    {
        float value = _gates.GetRequiredCrossbowDamageForBoss(bossZoneIndex);
        value *= GetLoopMultipliers(loopIndex).GateDamage;
        return Mathf.Max(0f, value);
    }

    #endregion

    #region Rewards API

    public MissionRewardResult GetMissionReward(
        MissionType missionType,
        int rewardIndex,
        WeaponClass missionWeaponClass,
        int loopIndex)
    {
        int weaponClassCount = System.Enum.GetValues(typeof(WeaponClass)).Length;
        return _rewards.GetMissionReward(
            missionType,
            rewardIndex,
            missionWeaponClass,
            weaponClassCount,
            _rounding,
            loopDeterminismSalt: loopIndex);
    }

    #endregion

    #region Generator helpers

    public float GetWeaponClassDamageMultiplier(WeaponClass weaponClass) => _weaponPerformance.GetDamageMultiplier(weaponClass);
    public RoundingModule Rounding => _rounding;

    #endregion
}
