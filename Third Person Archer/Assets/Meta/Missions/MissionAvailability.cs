public enum AvailabilityBlockReason
{
    None = 0,

    // Generic
    NoMissionSelected,

    // Campaign
    CampaignDamageTooLow,

    // Boss
    BossNotUnlocked,

    // Modes
    ContractsLockedByCompanyLevel,
    SniperLockedByCompanyLevel
}

public sealed class MissionAvailability
{
    public bool CanPlay { get; }
    public AvailabilityBlockReason Reason { get; }

    public MissionAvailability(bool canPlay, AvailabilityBlockReason reason)
    {
        CanPlay = canPlay;
        Reason = reason;
    }

    public static MissionAvailability Allowed() => new MissionAvailability(true, AvailabilityBlockReason.None);
    public static MissionAvailability Blocked(AvailabilityBlockReason reason) => new MissionAvailability(false, reason);
}