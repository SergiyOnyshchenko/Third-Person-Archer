using Meta.Weapons;

public sealed class MissionContext
{
    public ZoneData Zone { get; }
    public int ZoneIndex { get; }

    public MissionType SelectedType { get; }
    public MissionData Mission { get; }

    public int GlobalCampaignIndex { get; }
    public int CompanyLevel { get; }

    public WeaponClass RequiredWeaponClass { get; }

    // --- Debug/balance overrides ---
    public bool IsDebugRun { get; }
    public MissionType BalanceMode { get; } // what balance formulas should treat this as
    public int ContractsCompletedIndexOverride { get; } // -1 = no override

    /// <summary>Replay run info (Contracts = replay of a cleared level), null otherwise.</summary>
    public ContractRunInfo Contract { get; }
    public int ContractPlayerDamage => Contract != null ? Contract.PlayerDamage : 0;

    /// <summary>The whole campaign is done (any mission type): replays also use the Heroic tier and rewards.</summary>
    public bool IsHeroicEra { get; }
    /// <summary>Campaign level played in Heroic mode (whole campaign done): R from the Heroic tier.</summary>
    public bool IsHeroic { get; }
    /// <summary>Level position among same-class levels of the whole campaign (Heroic R curve).</summary>
    public float HeroicPosition01 { get; }

    public bool IsValid => Zone != null && Mission != null;

    public MissionContext(
        ZoneData zone,
        int zoneIndex,
        MissionType selectedType,
        MissionData mission,
        int globalCampaignIndex,
        int companyLevel,
        WeaponClass requiredWeaponClass,
        bool isDebugRun = false,
        MissionType? balanceMode = null,
        int contractsCompletedIndexOverride = -1,
        ContractRunInfo contract = null)
    {
        Zone = zone;
        ZoneIndex = zoneIndex;
        SelectedType = selectedType;
        Mission = mission;
        GlobalCampaignIndex = globalCampaignIndex;
        CompanyLevel = companyLevel;
        RequiredWeaponClass = requiredWeaponClass;

        IsDebugRun = isDebugRun;
        BalanceMode = balanceMode ?? selectedType;
        ContractsCompletedIndexOverride = contractsCompletedIndexOverride;
        Contract = contract;

        // Every Campaign context after the full campaign is Heroic (the normal campaign has nothing left).
        var progress = MissionProgressData.Active;
        IsHeroicEra = progress != null && progress.IsHeroicUnlocked();
        IsHeroic = selectedType == MissionType.Campaign && mission != null && IsHeroicEra;
        HeroicPosition01 = IsHeroic ? progress.GetHeroicPosition01(mission, requiredWeaponClass) : 0f;
    }
}

/// <summary>Replay run info fixed at launch (kept so the victory preview and the payout match).</summary>
public sealed class ContractRunInfo
{
    public int Slot { get; }
    public bool OrderMatched { get; }
    public bool DailyBonus { get; }
    public float RewardMultiplier { get; }
    public int PlayerDamage { get; }

    public ContractRunInfo(int slot, bool orderMatched, bool dailyBonus, float rewardMultiplier, int playerDamage)
    {
        Slot = slot;
        OrderMatched = orderMatched;
        DailyBonus = dailyBonus;
        RewardMultiplier = rewardMultiplier;
        PlayerDamage = playerDamage;
    }
}
