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
                // Replay of a cleared level: always open (the level was beaten once already).
                return ctx.IsValid
                    ? MissionAvailability.Allowed()
                    : MissionAvailability.Blocked(AvailabilityBlockReason.NoMissionSelected);

            // Sniper levels live on the Campaign path (soft gate there); there is no separate Sniper mode.
            default:
                return MissionAvailability.Blocked(AvailabilityBlockReason.NoMissionSelected);
        }
    }
}
