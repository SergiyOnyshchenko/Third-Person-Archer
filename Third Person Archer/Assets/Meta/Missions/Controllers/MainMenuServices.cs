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
        IWeaponRequirementService weaponRequirement)
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
    }

    public void NotifyMenuStateChanged()
    {
        OnMenuStateChanged?.Invoke();
    }
}