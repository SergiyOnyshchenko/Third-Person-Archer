using Meta.Weapons;

/// <summary>
/// Decides which mission type should display the recommended-hint indicator on the map.
/// Returns null when no single clear recommendation exists (e.g. zone with no boss after
/// campaign complete, or both Contracts and Sniper are locked).
///
/// Priority order:
/// 1. Campaign incomplete + gate passes           → Campaign
/// 2. Campaign incomplete + gate blocked          → Contracts (if unlocked), else Campaign
/// 3. Campaign complete + zone has boss + gate OK → Boss
/// 4. Campaign complete + boss gate blocked       → Sniper (if unlocked), else null
/// 5. Campaign complete + zone has no boss        → null (loop transition handles progression)
/// </summary>
public static class RecommendedMissionHintService
{
    public static MissionType? GetRecommendedType(MainMenuServices services)
    {
        if (services == null) return null;

        var zone = services.Progress.CurrentZone;
        if (zone == null) return null;

        if (!zone.IsCampaignComplete())
        {
            var campaignCtx = BuildContext(services, MissionType.Campaign);
            var campaignAvail = services.Availability.GetAvailability(campaignCtx);

            if (campaignAvail.CanPlay)
                return MissionType.Campaign;

            // Campaign gate blocked — Contracts is the primary token grind to clear it.
            var contractsCtx = BuildContext(services, MissionType.Contracts);
            var contractsAvail = services.Availability.GetAvailability(contractsCtx);
            if (contractsAvail.CanPlay)
                return MissionType.Contracts;

            // Contracts not yet unlocked by company level — Campaign remains the goal.
            return MissionType.Campaign;
        }

        // Campaign complete.
        if (!zone.HasBoss())
            return null; // Zone 3: loop transition determines next step, no hint shown.

        var bossCtx = BuildContext(services, MissionType.Boss);
        var bossAvail = services.Availability.GetAvailability(bossCtx);

        if (bossAvail.CanPlay)
            return MissionType.Boss;

        // Boss Crossbow gate blocked — Sniper gives CrossbowTokens needed to upgrade.
        if (bossAvail.Reason == AvailabilityBlockReason.BossCrossbowDamageTooLow)
        {
            var sniperCtx = BuildContext(services, MissionType.Sniper);
            var sniperAvail = services.Availability.GetAvailability(sniperCtx);
            if (sniperAvail.CanPlay)
                return MissionType.Sniper;
        }

        return null;
    }

    /// <summary>
    /// Builds a MissionContext for <paramref name="type"/> by cloning the current
    /// selected context and swapping the type and mission reference.
    /// This mirrors the same pattern used in MissionSelectorButtonPresenter.BuildContextFor.
    /// </summary>
    private static MissionContext BuildContext(MainMenuServices services, MissionType type)
    {
        var base_ = services.Context.BuildSelectedContext();
        var mission = services.Progress.GetMission(type);

        return new MissionContext(
            base_.Zone,
            base_.ZoneIndex,
            type,
            mission,
            base_.GlobalCampaignIndex,
            base_.CompanyLevel,
            base_.LoopIndex,
            base_.BalanceLoopIndex,
            base_.RequiredWeaponClass
        );
    }
}
