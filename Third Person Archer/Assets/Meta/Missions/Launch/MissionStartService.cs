using Meta.Weapons;

public enum MissionStartFailReason
{
    None = 0,
    NoContext,
    NotAvailable,
    NoEligibleContractMissions,
    MissingScene,
    ContractsUseGuildBoard, // Contracts start from the Guild Board (TryStartContract)
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

    /// <summary>Guild Board: replay the card's level with the class the player picked.</summary>
    MissionStartResult TryStartContract(ContractCard card, WeaponClass playedClass);
}

public sealed class MissionStartService : IMissionStartService
{
    private readonly MissionProgressData _progressData;
    private readonly IMissionContextService _context;
    private readonly IMissionAvailabilityService _availability;
    private readonly IMissionGateService _gate;
    private readonly IWeaponRequirementService _weaponRequirement;
    private readonly IContractBoardService _contractBoard;
    private readonly MissionLaunchRequest _launchRequest;
    private readonly MetaLoopProgressData _loopProgressData;

    public MissionStartService(
        MissionProgressData progressData,
        IMissionContextService context,
        IMissionAvailabilityService availability,
        IMissionGateService gate,
        IWeaponRequirementService weaponRequirement,
        IContractBoardService contractBoard,
        MissionLaunchRequest launchRequest,
        MetaLoopProgressData loopProgressData = null)
    {
        _progressData = progressData;
        _context = context;
        _availability = availability;
        _gate = gate;
        _weaponRequirement = weaponRequirement;
        _contractBoard = contractBoard;
        _launchRequest = launchRequest;
        _loopProgressData = loopProgressData;
    }

    public MissionStartResult TryStartSelected()
    {
        var ctx = _context.BuildSelectedContext();

        if (ctx == null)
            return MissionStartResult.Fail(MissionStartFailReason.NoContext);

        // Only Campaign / Boss / Sniper require a concrete Mission
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

            // If blocked by Sniper access gate, return gate info for popup.
            if (avail.Reason == AvailabilityBlockReason.SniperDamageTooLow)
            {
                var sniperGate = _gate.CheckSniperGate(ctx);
                return MissionStartResult.Fail(MissionStartFailReason.NotAvailable, avail, sniperGate);
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

            case MissionType.Sniper:
                missionToLoad = ctx.Mission;
                requiredClass = _weaponRequirement.GetRequiredWeaponClass(missionToLoad);
                break;

            case MissionType.Contracts:
                return MissionStartResult.Fail(MissionStartFailReason.ContractsUseGuildBoard, avail);

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

    public MissionStartResult TryStartContract(ContractCard card, WeaponClass playedClass)
    {
        var ctx = _context.BuildSelectedContext();
        if (ctx == null || card == null || card.Mission == null || _contractBoard == null)
            return MissionStartResult.Fail(MissionStartFailReason.NoEligibleContractMissions);

        // Mode lock (company level) is checked on a Contracts-typed context.
        var contractsCtx = new MissionContext(ctx.Zone, ctx.ZoneIndex, MissionType.Contracts, null,
            -1, ctx.CompanyLevel, playedClass);
        var avail = _availability.GetAvailability(contractsCtx);
        if (!avail.CanPlay)
            return MissionStartResult.Fail(MissionStartFailReason.NotAvailable, avail);

        if (!_contractBoard.IsClassPlayable(playedClass))
            return MissionStartResult.Fail(MissionStartFailReason.NotAvailable, avail);

        var mission = card.Mission;
        if (mission.Scene == null || string.IsNullOrEmpty(mission.Scene.ScenePath))
            return MissionStartResult.Fail(MissionStartFailReason.MissingScene, avail);

        // Zone = the level's zone (enemy R and reward); not a campaign run, so no global campaign index.
        _launchRequest.Set(
            mode: MissionType.Contracts,
            missionToLoad: mission,
            requiredWeaponClass: playedClass,
            zoneIndex: card.ZoneIndex,
            globalCampaignIndex: -1);
        _launchRequest.SetContract(_contractBoard.BuildRunInfo(card, playedClass));

        return MissionStartResult.Ok(mission, playedClass);
    }
}
