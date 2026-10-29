using System;
using UnityEngine;

public sealed class MainMenuServices
{
    public IMetaProgressWrite Progress { get; }
    public IMissionContextService Context { get; }
    public IMissionGateService Gate { get; }
    public IMissionAvailabilityService Availability { get; }
    public IMissionStartService MissionStart { get; }

    public MissionProgressData ProgressData { get; }
    public MissionLaunchRequest LaunchRequest { get; }
    public MetaLoopProgressData LoopData { get; }

    public IMissionCatalogService Catalog { get; }
    public IWeaponRequirementService WeaponRequirement { get; }
    public NextStepRecommendationService NextStep { get; }
    public BalanceConfig Balance { get; }
    public IMissionRewardService Rewards { get; }

    public event Action OnMenuStateChanged;

    public MainMenuServices(
        IMetaProgressWrite progress,
        IMissionContextService context,
        IMissionGateService gate,
        IMissionAvailabilityService availability,
        IMissionStartService missionStart,
        MissionProgressData progressData,
        MissionLaunchRequest launchRequest,
        MetaLoopProgressData loopData,
        IMissionCatalogService catalog,
        IWeaponRequirementService weaponRequirement,
        NextStepRecommendationService nextStep = null,
        BalanceConfig balance = null,
        IMissionRewardService rewards = null)
    {
        Progress = progress;
        Context = context;
        Gate = gate;
        Availability = availability;
        MissionStart = missionStart;

        ProgressData = progressData;
        LaunchRequest = launchRequest;
        LoopData = loopData;
        Catalog = catalog;
        WeaponRequirement = weaponRequirement;
        NextStep = nextStep;
        Balance = balance;
        Rewards = rewards;
    }

    /// <summary>
    /// Context for any mission of the current zone (not only the segment's current one):
    /// used by the map path and the mission panel to preview past/future levels.
    /// </summary>
    public MissionContext BuildContextFor(MissionType type, MissionData mission)
    {
        var baseCtx = Context.BuildSelectedContext();
        int globalIndex = Catalog.GetGlobalCampaignIndexOrMinusOne(mission);
        int companyLevel = globalIndex >= 0 ? globalIndex + 1 : ProgressData.GetCompanyLevel();

        return new MissionContext(
            baseCtx.Zone,
            baseCtx.ZoneIndex,
            type,
            mission,
            globalIndex,
            companyLevel,
            WeaponRequirement.GetRequiredWeaponClass(mission));
    }

    public void NotifyMenuStateChanged()
    {
        OnMenuStateChanged?.Invoke();
    }
}