using System.Collections.Generic;

public interface IMissionCatalogService
{
    IReadOnlyList<MissionData> GlobalCampaignMissions { get; }

    bool TryGetGlobalCampaignIndex(MissionData mission, out int index);

    /// <summary>Convenience: returns -1 if mission is not in global campaign list.</summary>
    int GetGlobalCampaignIndexOrMinusOne(MissionData mission);
}
