public sealed class MissionCompletionService : IMissionCompletionService
{
    private readonly IMetaProgressWrite _progress;
    private readonly MetaLoopProgressData _loopData;
    private readonly IMissionRewardService _rewards;

    public MissionCompletionService(
        IMetaProgressWrite progress,
        MetaLoopProgressData loopData,
        IMissionRewardService rewards)
    {
        _progress = progress;
        _loopData = loopData;
        _rewards = rewards;
    }

    public MissionCompleteResult Complete(MissionContext ctx, MissionOutcome outcome)
    {
        if (ctx == null || !ctx.IsValid)
            return new MissionCompleteResult(false, new MissionReward(0, null));

        if (outcome != MissionOutcome.Completed)
            return new MissionCompleteResult(false, new MissionReward(0, null));

        var reward = _rewards.Calculate(ctx);

        if (!ctx.IsDebugRun)
            ApplyProgress(ctx);

        return new MissionCompleteResult(true, reward);
    }

    private void ApplyProgress(MissionContext ctx)
    {
        switch (ctx.SelectedType)
        {
            case MissionType.Campaign:
                ctx.Zone.AdvanceMission(MissionType.Campaign);
                break;

            case MissionType.Boss:
                ctx.Zone.AdvanceMission(MissionType.Boss);
                // After boss -> move to next zone or next loop.
                AdvanceToNextZoneOrLoop(ctx.ZoneIndex);
                break;

            case MissionType.Contracts:
                _loopData?.IncreaseContractsCompleted();
                break;

            case MissionType.Sniper:
                _loopData?.IncreaseSniperCompleted();
                break;
        }
    }

    private void AdvanceToNextZoneOrLoop(int currentZoneIndex)
    {
        var zones = _progress.AllZones;
        if (zones == null || zones.Count == 0)
            return;

        bool isLastZone = currentZoneIndex >= zones.Count - 1;

        if (!isLastZone)
        {
            _progress.SelectZone(currentZoneIndex + 1);
            _progress.SelectMissionType(MissionType.Campaign);
            return;
        }

        // Finished last zone boss => finished the run => next loop.
        _loopData?.OnFullCampaignAndAllBossesCompleted();

        // Restart from zone 0 (first zone) at next loop.
        _progress.SelectZone(0);
        _progress.SelectMissionType(MissionType.Campaign);
    }
}