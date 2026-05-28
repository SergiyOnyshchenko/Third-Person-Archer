public sealed class MissionAvailabilityService : IMissionAvailabilityService
{
    private readonly IMissionGateService _gateService;
    private readonly MetaModeUnlockConfig _modeUnlocks;
    private readonly ICompanyLevelService _companyLevel;
    private readonly MetaLoopProgressData _loopProgressData;

    public MissionAvailabilityService(
        IMissionGateService gateService,
        MetaModeUnlockConfig modeUnlocks,
        ICompanyLevelService companyLevel,
        MetaLoopProgressData loopProgressData)
    {
        _gateService = gateService;
        _modeUnlocks = modeUnlocks;
        _companyLevel = companyLevel;
        _loopProgressData = loopProgressData;
    }

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

                if (!ctx.Zone.IsBossUnlocked())
                    return MissionAvailability.Blocked(AvailabilityBlockReason.BossNotUnlocked);

                var bossGate = _gateService.CheckBossGate(ctx);
                return bossGate.Passed
                    ? MissionAvailability.Allowed()
                    : MissionAvailability.Blocked(AvailabilityBlockReason.BossCrossbowDamageTooLow);

            case MissionType.Contracts:
                return companyLevel >= _modeUnlocks.ContractsUnlockCompanyLevel
                    ? MissionAvailability.Allowed()
                    : MissionAvailability.Blocked(AvailabilityBlockReason.ContractsLockedByCompanyLevel);

            case MissionType.Sniper:
                {
                    if (companyLevel < _modeUnlocks.SniperUnlockCompanyLevel)
                        return MissionAvailability.Blocked(AvailabilityBlockReason.SniperLockedByCompanyLevel);

                    // Check Crossbow damage access gate for current Sniper tier.
                    int sniperIndex = _loopProgressData != null ? _loopProgressData.SniperCompletedIndex : 0;
                    var sniperGate = _gateService.CheckSniperGate(ctx, sniperIndex);
                    return sniperGate.Passed
                        ? MissionAvailability.Allowed()
                        : MissionAvailability.Blocked(AvailabilityBlockReason.SniperDamageTooLow);
                }

            default:
                return MissionAvailability.Blocked(AvailabilityBlockReason.NoMissionSelected);
        }
    }
}
