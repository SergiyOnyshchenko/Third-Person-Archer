using System;

public static class CampaignLevelFormatUtility
{
    /// <summary>
    /// Converts CompanyLevel (1-based) to "ZoneNumber-CampaignInZone" (both 1-based).
    /// ZoneNumber is loop-aware: Loop2 Zone3 => ZoneNumber=6 (if zonesPerLoop=3).
    /// Examples: "3-1", "6-2".
    /// </summary>
    public static string FormatCompanyLevelAsZoneCampaign(MissionProgressData progressData, int companyLevel)
    {
        if (companyLevel <= 0)
            return "0-0";

        if (progressData == null || progressData.AllZones == null || progressData.AllZones.Count == 0)
            return companyLevel.ToString();

        int campaignsPerLoop = GetCampaignsPerLoop(progressData);
        if (campaignsPerLoop <= 0)
            return companyLevel.ToString();

        int globalCampaignIndex = companyLevel - 1;     // 0-based
        int loopIndex = globalCampaignIndex / campaignsPerLoop;
        int indexInLoop = globalCampaignIndex % campaignsPerLoop;

        int zoneWithinLoop = 0;
        int campaignIndexInZone = 0;

        var zones = progressData.AllZones;

        // Find which authored zone contains this campaign index within the loop.
        for (int z = 0; z < zones.Count; z++)
        {
            int zoneCampaignCount = GetCampaignCount(zones[z]);
            if (zoneCampaignCount <= 0)
                continue;

            if (indexInLoop < zoneCampaignCount)
            {
                zoneWithinLoop = z;
                campaignIndexInZone = indexInLoop;
                break;
            }

            indexInLoop -= zoneCampaignCount;
        }

        int zonesPerLoop = zones.Count;
        int displayZoneNumber = loopIndex * zonesPerLoop + zoneWithinLoop + 1;  // 1-based, loop-aware
        int displayCampaignNumber = campaignIndexInZone + 1;                    // 1-based

        return $"{displayZoneNumber}-{displayCampaignNumber}";
    }

    private static int GetCampaignsPerLoop(MissionProgressData progressData)
    {
        int total = 0;
        var zones = progressData.AllZones;

        for (int i = 0; i < zones.Count; i++)
            total += GetCampaignCount(zones[i]);

        return total;
    }

    private static int GetCampaignCount(ZoneData zone)
    {
        if (zone == null)
            return 0;

        var segment = zone.GetSegmentByType(MissionType.Campaign);
        return segment?.Missions != null ? segment.Missions.Count : 0;
    }
}