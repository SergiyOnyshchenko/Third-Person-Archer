using Meta.Weapons;
using UnityEngine;

public enum MissionStartFailReason
{
    None = 0,
    NoContext,
    NotAvailable,
    NoEligibleContractMissions,
    MissingScene,
    ReplayNeedsLevel, // Contracts = replay of a cleared level: start it with TryStartReplay
}

public sealed class MissionStartResult
{
    public bool Started { get; }
    public MissionStartFailReason FailReason { get; }

    public MissionAvailability Availability { get; }
    public MissionGateResult Gate { get; }

    public MissionData MissionToLoad { get; }
    public WeaponClass RequiredWeaponClass { get; }

    public MissionStartResult(
        bool started,
        MissionStartFailReason failReason,
        MissionAvailability availability,
        MissionGateResult gate,
        MissionData missionToLoad,
        WeaponClass requiredWeaponClass)
    {
        Started = started;
        FailReason = failReason;
        Availability = availability;
        Gate = gate;
        MissionToLoad = missionToLoad;
        RequiredWeaponClass = requiredWeaponClass;
    }

    public static MissionStartResult Fail(MissionStartFailReason reason, MissionAvailability availability = null, MissionGateResult gate = null)
        => new MissionStartResult(false, reason, availability, gate, null, default);

    public static MissionStartResult Ok(MissionData missionToLoad, WeaponClass requiredWeaponClass)
        => new MissionStartResult(true, MissionStartFailReason.None, MissionAvailability.Allowed(), null, missionToLoad, requiredWeaponClass);
}

public interface IMissionStartService
{
    MissionStartResult TryStartSelected();

    /// <summary>Replay a cleared path level with its own class: gold + essence, no path progress.</summary>
    MissionStartResult TryStartReplay(MissionData mission, int zoneIndex);

    /// <summary>Replay the last cleared level of the class (stuck on a gate: farm its essence).</summary>
    MissionStartResult TryStartReplay(WeaponClass weaponClass);
}

public sealed class MissionStartService : IMissionStartService
{
    private readonly MissionProgressData _progressData;
    private readonly IMissionContextService _context;
    private readonly IMissionAvailabilityService _availability;
    private readonly IMissionGateService _gate;
    private readonly IWeaponRequirementService _weaponRequirement;
    private readonly ILoadoutWeaponStatService _weaponStats;
    private readonly MissionLaunchRequest _launchRequest;
    private readonly MetaLoopProgressData _loopProgressData;

    public MissionStartService(
        MissionProgressData progressData,
        IMissionContextService context,
        IMissionAvailabilityService availability,
        IMissionGateService gate,
        IWeaponRequirementService weaponRequirement,
        ILoadoutWeaponStatService weaponStats,
        MissionLaunchRequest launchRequest,
        MetaLoopProgressData loopProgressData = null)
    {
        _progressData = progressData;
        _context = context;
        _availability = availability;
        _gate = gate;
        _weaponRequirement = weaponRequirement;
        _weaponStats = weaponStats;
        _launchRequest = launchRequest;
        _loopProgressData = loopProgressData;
    }

    public MissionStartResult TryStartSelected()
    {
        var ctx = _context.BuildSelectedContext();

        if (ctx == null)
            return MissionStartResult.Fail(MissionStartFailReason.NoContext);

        if (ctx.SelectedType != MissionType.Contracts && !ctx.IsValid)
            return MissionStartResult.Fail(MissionStartFailReason.NoContext);

        var avail = _availability.GetAvailability(ctx);
        if (!avail.CanPlay)
        {
            // If blocked by Campaign damage gate, return gate info for popup.
            if (avail.Reason == AvailabilityBlockReason.CampaignDamageTooLow)
            {
                var gate = _gate.CheckCampaignGate(ctx);
                return MissionStartResult.Fail(MissionStartFailReason.NotAvailable, avail, gate);
            }

            // If blocked by Boss Crossbow gate, return gate info for popup.
            if (avail.Reason == AvailabilityBlockReason.BossCrossbowDamageTooLow)
            {
                var bossGate = _gate.CheckBossGate(ctx);
                return MissionStartResult.Fail(MissionStartFailReason.NotAvailable, avail, bossGate);
            }

            return MissionStartResult.Fail(MissionStartFailReason.NotAvailable, avail);
        }

        // Resolve which MissionData scene we actually load + required weapon class.
        MissionData missionToLoad;
        WeaponClass requiredClass;

        switch (ctx.SelectedType)
        {
            case MissionType.Campaign:
                missionToLoad = ctx.Mission;
                requiredClass = _weaponRequirement.GetRequiredWeaponClass(missionToLoad);
                break;

            case MissionType.Boss:
                missionToLoad = ctx.Mission;
                requiredClass = _weaponRequirement.GetRequiredWeaponClass(missionToLoad);
                break;

            case MissionType.Contracts:
                return MissionStartResult.Fail(MissionStartFailReason.ReplayNeedsLevel, avail);

            default:
                return MissionStartResult.Fail(MissionStartFailReason.NoContext);
        }

        if (missionToLoad == null || missionToLoad.Scene == null || string.IsNullOrEmpty(missionToLoad.Scene.ScenePath))
            return MissionStartResult.Fail(MissionStartFailReason.MissingScene, avail);

        int globalIndex = ctx.GlobalCampaignIndex;
        _launchRequest.Set(
            mode: ctx.SelectedType,
            missionToLoad: missionToLoad,
            requiredWeaponClass: requiredClass,
            zoneIndex: ctx.ZoneIndex,
            globalCampaignIndex: globalIndex);

        return MissionStartResult.Ok(missionToLoad, requiredClass);
    }

    public MissionStartResult TryStartReplay(WeaponClass weaponClass)
    {
        return _progressData != null && _progressData.TryFindReplayLevel(weaponClass, out var mission, out int zoneIndex)
            ? TryStartReplay(mission, zoneIndex)
            : MissionStartResult.Fail(MissionStartFailReason.NoEligibleContractMissions);
    }

    public MissionStartResult TryStartReplay(MissionData mission, int zoneIndex)
    {
        var zones = _progressData != null ? _progressData.AllZones : null;
        if (mission == null || zones == null || zoneIndex < 0 || zoneIndex >= zones.Count)
            return MissionStartResult.Fail(MissionStartFailReason.NoEligibleContractMissions);

        var cls = _weaponRequirement.GetRequiredWeaponClass(mission);
        var ctx = new MissionContext(zones[zoneIndex], zoneIndex, MissionType.Contracts, mission, -1,
            _progressData.GetCompanyLevel(), cls);

        var avail = _availability.GetAvailability(ctx);
        if (!avail.CanPlay)
            return MissionStartResult.Fail(MissionStartFailReason.NotAvailable, avail);

        if (mission.Scene == null || string.IsNullOrEmpty(mission.Scene.ScenePath))
            return MissionStartResult.Fail(MissionStartFailReason.MissingScene, avail);

        // Zone = the level's zone (enemy R and reward); not a path run, so no global campaign index.
        // Player damage caps enemy HP (a replay never needs more than MaxBodyShots body shots).
        int damage = _weaponStats != null ? Mathf.FloorToInt(_weaponStats.GetEquippedDamage(cls)) : 0;
        _launchRequest.Set(
            mode: MissionType.Contracts,
            missionToLoad: mission,
            requiredWeaponClass: cls,
            zoneIndex: zoneIndex,
            globalCampaignIndex: -1);
        _launchRequest.SetContract(new ContractRunInfo(-1, false, false, 1f, damage));

        return MissionStartResult.Ok(mission, cls);
    }
}
