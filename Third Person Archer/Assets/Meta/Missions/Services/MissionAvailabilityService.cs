public sealed class MissionAvailabilityService : IMissionAvailabilityService
{
    private readonly IMissionGateService _gateService;
    private readonly MetaModeUnlockConfig _modeUnlocks;
    private readonly ICompanyLevelService _companyLevel;

    public MissionAvailabilityService(
        IMissionGateService gateService,
        MetaModeUnlockConfig modeUnlocks,
        ICompanyLevelService companyLevel)
    {
        _gateService = gateService;
        _modeUnlocks = modeUnlocks;
        _companyLevel = companyLevel;
    }

    /*
        public MissionAvailability GetAvailability(MissionContext ctx)
        {
            if (ctx == null || !ctx.IsValid)
                return MissionAvailability.Blocked(AvailabilityBlockReason.NoMissionSelected);

            int companyLevel = _companyLevel.GetCompanyLevel();

            switch (ctx.SelectedType)
            {
                case MissionType.Campaign:
                {
                    var gate = _gateService.CheckCampaignGate(ctx);
                    return gate.Passed
                        ? MissionAvailability.Allowed()
                        : MissionAvailability.Blocked(AvailabilityBlockReason.CampaignDamageTooLow);
                }
                case MissionType.Boss:
                    // Boss unlocked = all campaign missions completed in that zone (your ZoneData already has IsBossUnlocked()).
                    return ctx.Zone.IsBossUnlocked()
                        ? MissionAvailability.Allowed()
                        : MissionAvailability.Blocked(AvailabilityBlockReason.BossNotUnlocked);

                case MissionType.Contracts:
                    return companyLevel >= _modeUnlocks.ContractsUnlockCompanyLevel
                        ? MissionAvailability.Allowed()
                        : MissionAvailability.Blocked(AvailabilityBlockReason.ContractsLockedByCompanyLevel);

                case MissionType.Sniper:
                    return companyLevel >= _modeUnlocks.SniperUnlockCompanyLevel
                        ? MissionAvailability.Allowed()
                        : MissionAvailability.Blocked(AvailabilityBlockReason.SniperLockedByCompanyLevel);

                default:
                    return MissionAvailability.Blocked(AvailabilityBlockReason.NoMissionSelected);
            }
        }
    */

    public MissionAvailability GetAvailability(MissionContext ctx)
    {
        if (ctx == null)
            return MissionAvailability.Blocked(AvailabilityBlockReason.NoMissionSelected);

        int companyLevel = _companyLevel.GetCompanyLevel();

        switch (ctx.SelectedType)
        {
            case MissionType.Campaign:
                {
                    if (!ctx.IsValid)
                        return MissionAvailability.Blocked(AvailabilityBlockReason.NoMissionSelected);

                    var gate = _gateService.CheckCampaignGate(ctx);
                    return gate.Passed
                        ? MissionAvailability.Allowed()
                        : MissionAvailability.Blocked(AvailabilityBlockReason.CampaignDamageTooLow);
                }

            case MissionType.Boss:
                if (!ctx.IsValid)
                    return MissionAvailability.Blocked(AvailabilityBlockReason.NoMissionSelected);

                return ctx.Zone.IsBossUnlocked()
                    ? MissionAvailability.Allowed()
                    : MissionAvailability.Blocked(AvailabilityBlockReason.BossNotUnlocked);

            case MissionType.Contracts:
                // Contracts does NOT need a concrete mission selected to be "available".
                return companyLevel >= _modeUnlocks.ContractsUnlockCompanyLevel
                    ? MissionAvailability.Allowed()
                    : MissionAvailability.Blocked(AvailabilityBlockReason.ContractsLockedByCompanyLevel);

            case MissionType.Sniper:
                // Same here.
                return companyLevel >= _modeUnlocks.SniperUnlockCompanyLevel
                    ? MissionAvailability.Allowed()
                    : MissionAvailability.Blocked(AvailabilityBlockReason.SniperLockedByCompanyLevel);

            default:
                return MissionAvailability.Blocked(AvailabilityBlockReason.NoMissionSelected);
        }
    }
}