using Meta.Weapons;

public sealed class MissionContext
{
    public ZoneData Zone { get; }
    public int ZoneIndex { get; }

    public MissionType SelectedType { get; }
    public MissionData Mission { get; }

    public int GlobalCampaignIndex { get; }
    public int CompanyLevel { get; }

    public int LoopIndex { get; }
    public int BalanceLoopIndex { get; }

    public WeaponClass RequiredWeaponClass { get; }

    // --- Debug/balance overrides ---
    public bool IsDebugRun { get; }
    public MissionType BalanceMode { get; } // what balance formulas should treat this as
    public int ContractsCompletedIndexOverride { get; } // -1 = no override

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
        WeaponClass requiredWeaponClass,
        bool isDebugRun = false,
        MissionType? balanceMode = null,
        int contractsCompletedIndexOverride = -1)
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

        IsDebugRun = isDebugRun;
        BalanceMode = balanceMode ?? selectedType;
        ContractsCompletedIndexOverride = contractsCompletedIndexOverride;
    }
}