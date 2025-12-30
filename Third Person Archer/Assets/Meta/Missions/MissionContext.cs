using Meta.Weapons;


public sealed class MissionContext
{
    public ZoneData Zone { get; }
    public int ZoneIndex { get; }

    public MissionType SelectedType { get; }
    public MissionData Mission { get; }

    /// <summary>Global campaign index (only meaningful if Mission is Campaign, or if Mission is a Campaign scene reused by contracts).</summary>
    public int GlobalCampaignIndex { get; }

    /// <summary>Company level is always globalCampaignIndex + 1 when meaningful. If not meaningful, this can be computed from progress.</summary>
    public int CompanyLevel { get; }

    public int LoopIndex { get; }
    public int BalanceLoopIndex { get; }

    public WeaponClass RequiredWeaponClass { get; }

    public bool IsValid => Zone != null && Mission != null;

    public MissionContext(
        ZoneData zone,
        int zoneIndex,
        MissionType selectedType,
        MissionData mission,
        int globalCampaignIndex,
        int companyLevel,
        int loopIndex,
        int balanceLoopIndex,
        WeaponClass requiredWeaponClass)
    {
        Zone = zone;
        ZoneIndex = zoneIndex;
        SelectedType = selectedType;
        Mission = mission;
        GlobalCampaignIndex = globalCampaignIndex;
        CompanyLevel = companyLevel;
        LoopIndex = loopIndex;
        BalanceLoopIndex = balanceLoopIndex;
        RequiredWeaponClass = requiredWeaponClass;
    }
}