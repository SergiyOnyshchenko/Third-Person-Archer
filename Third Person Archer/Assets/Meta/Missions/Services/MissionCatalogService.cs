using System.Collections.Generic;
using UnityEngine;

public sealed class MissionCatalogService : IMissionCatalogService
{
    private readonly List<MissionData> _globalCampaign = new List<MissionData>();
    private readonly Dictionary<MissionData, int> _globalIndexByMission = new Dictionary<MissionData, int>();

    public IReadOnlyList<MissionData> GlobalCampaignMissions => _globalCampaign;

    public MissionCatalogService(IReadOnlyList<ZoneData> zonesInOrder)
    {
        Build(zonesInOrder);
    }

    private void Build(IReadOnlyList<ZoneData> zonesInOrder)
    {
        _globalCampaign.Clear();
        _globalIndexByMission.Clear();

        if (zonesInOrder == null)
            return;

        for (int zi = 0; zi < zonesInOrder.Count; zi++)
        {
            var zone = zonesInOrder[zi];
            if (zone == null) continue;

            var campaignSegment = zone.GetSegmentByType(MissionType.Campaign);
            if (campaignSegment == null) continue;

            // Assumption: MissionSegmentData exposes Missions list (as in your existing design).
            var missions = campaignSegment.Missions;
            if (missions == null) continue;

            for (int mi = 0; mi < missions.Count; mi++)
            {
                var mission = missions[mi];
                if (mission == null) continue;

                // Deduplicate defensively.
                if (_globalIndexByMission.ContainsKey(mission))
                {
                    Debug.LogWarning($"Duplicate campaign mission detected in catalog: {mission.name}");
                    continue;
                }

                int index = _globalCampaign.Count;
                _globalCampaign.Add(mission);
                _globalIndexByMission.Add(mission, index);
            }
        }
    }

    public bool TryGetGlobalCampaignIndex(MissionData mission, out int index)
    {
        if (mission == null)
        {
            index = -1;
            return false;
        }

        return _globalIndexByMission.TryGetValue(mission, out index);
    }

    public int GetGlobalCampaignIndexOrMinusOne(MissionData mission)
    {
        return TryGetGlobalCampaignIndex(mission, out var idx) ? idx : -1;
    }
}