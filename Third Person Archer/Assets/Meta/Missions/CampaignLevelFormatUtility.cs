public static class CampaignLevelFormatUtility
{
    /// <summary>
    /// Converts CompanyLevel (1-based) to "ZoneNumber-CampaignInZone" (both 1-based), e.g. "2-3".
    /// Levels past the campaign are clamped to its last level.
    /// </summary>
    public static string FormatCompanyLevelAsZoneCampaign(MissionProgressData progressData, int companyLevel)
    {
        if (companyLevel <= 0)
            return "0-0";

        if (progressData == null || progressData.AllZones == null || progressData.AllZones.Count == 0)
            return companyLevel.ToString();

        var zones = progressData.AllZones;
        int index = companyLevel - 1; // 0-based global campaign index
        int lastZone = -1, lastCount = 0;

        for (int z = 0; z < zones.Count; z++)
        {
            int count = GetCampaignCount(zones[z]);
            if (count <= 0)
                continue;

            if (index < count)
                return $"{z + 1}-{index + 1}";

            index -= count;
            lastZone = z;
            lastCount = count;
        }

        return lastZone >= 0 ? $"{lastZone + 1}-{lastCount}" : companyLevel.ToString();
    }

    private static int GetCampaignCount(ZoneData zone)
    {
        var segment = zone != null ? zone.GetSegmentByType(MissionType.Campaign) : null;
        return segment?.Missions != null ? segment.Missions.Count : 0;
    }
}
