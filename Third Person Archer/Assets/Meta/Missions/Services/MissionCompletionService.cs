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
                if (ctx.IsHeroic)
                {
                    _progress.AdvanceHeroic(ctx.Zone);
                    break;
                }

                ctx.Zone.AdvanceMission(MissionType.Campaign);
                if (_progress.IsHeroicUnlocked)
                {
                    // Campaign finale: Heroic mode starts from the first zone.
                    _progress.SelectZone(0);
                    _progress.SelectMissionType(MissionType.Campaign);
                }
                break;

            case MissionType.Boss:
                ctx.Zone.AdvanceMission(MissionType.Boss);
                AdvanceToNextZone(ctx.ZoneIndex);
                break;

            case MissionType.Contracts:
                // Replay: no path progress.
                _loopData?.IncreaseContractsCompleted();
                break;

            case MissionType.Sniper:
                ctx.Zone.AdvanceMission(MissionType.Sniper);
                _loopData?.IncreaseSniperCompleted();
                break;
        }
    }

    private void AdvanceToNextZone(int currentZoneIndex)
    {
        var zones = _progress.AllZones;
        if (zones == null || zones.Count == 0)
            return;

        // The last zone has no Boss: the campaign ends on its last level and Heroic mode follows
        // (campaign loops are gone). A boss replay never moves the player forward past the last zone.
        if (currentZoneIndex >= zones.Count - 1)
            return;

        // Replaying an old boss (Heroic, all zones open) must not move the map.
        if (_progress.IsHeroicUnlocked)
            return;

        _progress.SelectZone(currentZoneIndex + 1);
        _progress.SelectMissionType(MissionType.Campaign);
    }
}
