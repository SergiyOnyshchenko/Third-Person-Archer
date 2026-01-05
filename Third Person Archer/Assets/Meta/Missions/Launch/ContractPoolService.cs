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

    /*
        public MissionData PickRandomEligibleCampaignMission()
        {
            IReadOnlyList<MissionData> eligible = _progress.GetEligibleContractCampaignMissions();

            if (eligible == null || eligible.Count == 0)
                return null;

            int idx = UnityEngine.Random.Range(0, eligible.Count);
            return eligible[idx];
        }
    */

    public MissionData PickRandomEligibleCampaignMission()
    {
        var eligible = _progress.GetEligibleContractCampaignMissions();

        //Debug.Log($"[Contracts] Eligible campaign missions: {(eligible == null ? -1 : eligible.Count)} | CompanyLevel={_progress.GetCompanyLevel()}");

        // Optional: print campaign completion per zone
        foreach (var z in _progress.AllZones)
        {
            var seg = z.GetSegmentByType(MissionType.Campaign);
            //Debug.Log($"[Contracts] Zone={z.ID} CampaignCompletedOnce={seg.GetCompletedOnceCount()} / {seg.Missions.Count}");
        }

        if (eligible == null || eligible.Count == 0)
            return null;

        return eligible[UnityEngine.Random.Range(0, eligible.Count)];
    }
}