using Meta.Weapons;

public enum MissionStartFailReason
{
    None = 0,
    NoContext,
    NotAvailable,
    NoEligibleContractMissions,
    MissingScene,
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
}

public sealed class MissionStartService : IMissionStartService
{
    private readonly MissionProgressData _progressData;
    private readonly IMissionContextService _context;
    private readonly IMissionAvailabilityService _availability;
    private readonly IMissionGateService _gate;
    private readonly IWeaponRequirementService _weaponRequirement;
    private readonly IContractPoolService _contractPool;
    private readonly MissionLaunchRequest _launchRequest;

    // Contracts: you said weapon types can be random for variety.
    // Keep it simple: randomize required weapon class for contracts.
    private readonly bool _randomizeContractWeaponClass = true;

    public MissionStartService(
        MissionProgressData progressData,
        IMissionContextService context,
        IMissionAvailabilityService availability,
        IMissionGateService gate,
        IWeaponRequirementService weaponRequirement,
        IContractPoolService contractPool,
        MissionLaunchRequest launchRequest)
    {
        _progressData = progressData;
        _context = context;
        _availability = availability;
        _gate = gate;
        _weaponRequirement = weaponRequirement;
        _contractPool = contractPool;
        _launchRequest = launchRequest;
    }

    public MissionStartResult TryStartSelected()
    {
        var ctx = _context.BuildSelectedContext();
        if (ctx == null || !ctx.IsValid)
            return MissionStartResult.Fail(MissionStartFailReason.NoContext);

        var avail = _availability.GetAvailability(ctx);
        if (!avail.CanPlay)
        {
            // If blocked by damage gate, return gate info for popup.
            if (avail.Reason == AvailabilityBlockReason.CampaignDamageTooLow)
            {
                var gate = _gate.CheckCampaignGate(ctx);
                return MissionStartResult.Fail(MissionStartFailReason.NotAvailable, avail, gate);
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
                requiredClass = _weaponRequirement.GetRequiredWeaponClass(missionToLoad, ctx.LoopIndex);
                break;

            case MissionType.Boss:
                missionToLoad = ctx.Mission;
                requiredClass = _weaponRequirement.GetRequiredWeaponClass(missionToLoad, ctx.LoopIndex);
                break;

            case MissionType.Sniper:
                // Sniper is special: fixed weapon class is already on MissionData.
                missionToLoad = ctx.Mission;
                requiredClass = _weaponRequirement.GetRequiredWeaponClass(missionToLoad, ctx.LoopIndex);
                break;

            case MissionType.Contracts:
                missionToLoad = _contractPool.PickRandomEligibleCampaignMission();
                if (missionToLoad == null)
                    return MissionStartResult.Fail(MissionStartFailReason.NoEligibleContractMissions, avail);

                requiredClass = _randomizeContractWeaponClass
                    ? GetRandomWeaponClass()
                    : _weaponRequirement.GetRequiredWeaponClass(missionToLoad, ctx.LoopIndex);
                break;

            default:
                return MissionStartResult.Fail(MissionStartFailReason.NoContext);
        }

        if (missionToLoad == null || missionToLoad.Scene == null || string.IsNullOrEmpty(missionToLoad.Scene.ScenePath))
            return MissionStartResult.Fail(MissionStartFailReason.MissingScene, avail);

        // Store request for gameplay scene.
        int globalIndex = ctx.GlobalCampaignIndex;
        _launchRequest.Set(
            mode: ctx.SelectedType,
            missionToLoad: missionToLoad,
            requiredWeaponClass: requiredClass,
            zoneIndex: ctx.ZoneIndex,
            globalCampaignIndex: globalIndex,
            loopIndex: ctx.LoopIndex,
            balanceLoopIndex: ctx.BalanceLoopIndex
        );

        return MissionStartResult.Ok(missionToLoad, requiredClass);
    }

    private static WeaponClass GetRandomWeaponClass()
    {
        int count = System.Enum.GetValues(typeof(WeaponClass)).Length;
        int idx = UnityEngine.Random.Range(0, count);
        return (WeaponClass)idx;
    }
}