using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public sealed class MainMenuServices
{
    public IMetaProgressWrite Progress { get; }
    public IMissionContextService Context { get; }
    public IMissionGateService Gate { get; }
    public IMissionAvailabilityService Availability { get; }
    public IMissionStartService MissionStart { get; }

    public event Action OnMenuStateChanged;

    public MainMenuServices(
        IMetaProgressWrite progress,
        IMissionContextService context,
        IMissionGateService gate,
        IMissionAvailabilityService availability,
        IMissionStartService missionStart)
    {
        Progress = progress;
        Context = context;
        Gate = gate;
        Availability = availability;
        MissionStart = missionStart;
    }

    public void NotifyMenuStateChanged()
    {
        OnMenuStateChanged?.Invoke();
    }
}