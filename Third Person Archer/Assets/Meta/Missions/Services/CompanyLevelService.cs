public sealed class CompanyLevelService : ICompanyLevelService
{
    private readonly IMetaProgressReadOnly _progress;

    public CompanyLevelService(IMetaProgressReadOnly progress)
    {
        _progress = progress;
    }

    public int GetCompanyLevel()
    {
        // Company level = number of completed campaign missions across ALL zones + 1.
        int completedCampaign = 0;

        var zones = _progress.AllZones;
        if (zones != null)
        {
            for (int zi = 0; zi < zones.Count; zi++)
            {
                var zone = zones[zi];
                if (zone == null) continue;

                var campaignSeg = zone.GetSegmentByType(MissionType.Campaign);
                if (campaignSeg == null) continue;

                completedCampaign += campaignSeg.GetCompletedOnceCount();
            }
        }

        return completedCampaign + 1;
    }
}