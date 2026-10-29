using Meta.Weapons;

/// <summary>
/// Decides which mission type should display the recommended-hint indicator on the map.
/// Returns null when no single clear recommendation exists (e.g. zone with no boss after
/// campaign complete).
///
/// Priority order:
/// 1. Campaign path incomplete                    → Campaign
/// 2. Campaign complete + zone has boss           → Boss
/// 3. Campaign complete + zone has no boss        → null
/// Heroic mode (whole campaign done): treated as campaign incomplete (rules 1–2).
/// </summary>
public static class RecommendedMissionHintService
{
    public static MissionType? GetRecommendedType(MainMenuServices services)
    {
        if (services == null) return null;

        var zone = services.Progress.CurrentZone;
        if (zone == null) return null;

        // Heroic mode: the Campaign always has a next level (it starts over after the last one).
        bool heroic = services.ProgressData != null && services.ProgressData.IsHeroicUnlocked();
        if (heroic || !zone.IsCampaignComplete())
        {
            // Gate blocked: the Play button still leads there and its popup explains how to get stronger.
            return MissionType.Campaign;
        }

        // Campaign complete.
        if (!zone.HasBoss())
            return null; // Zone 3: loop transition determines next step, no hint shown.

        var bossCtx = BuildContext(services, MissionType.Boss);
        var bossAvail = services.Availability.GetAvailability(bossCtx);

        return bossAvail.CanPlay || bossAvail.Reason == AvailabilityBlockReason.BossCrossbowDamageTooLow
            ? MissionType.Boss
            : null;
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
            base_.RequiredWeaponClass
        );
    }
}
