using System;
using System.Collections.Generic;
using UnityEngine;

public interface IContractPoolService
{
    MissionData PickRandomEligibleCampaignMission();
}

public sealed class ContractPoolService : IContractPoolService
{
    private readonly MissionProgressData _progress;

    public ContractPoolService(MissionProgressData progress)
    {
        _progress = progress;
    }

    public MissionData PickRandomEligibleCampaignMission()
    {
        IReadOnlyList<MissionData> eligible = _progress.GetEligibleContractCampaignMissions();

        if (eligible == null || eligible.Count == 0)
            return null;

        int idx = UnityEngine.Random.Range(0, eligible.Count);
        return eligible[idx];
    }
}