public static class MissionUnlockService
{
    public static bool CanUnlock(MissionData mission, MissionSegmentData segment, ZoneData zone)
    {
        if (mission == null || segment == null || zone == null)
            return false;

        int missionIndex = segment.GetMissionIndex(mission);

        if (segment.UnlockCondition == null || missionIndex < segment.UnlockCondition.UnlockAfterIndex)
            return true;

        var tracker = new MissionProgressTracker(zone);

        if (segment.UnlockCondition.LogicType == UnlockLogicType.All)
        {
            // AND logic: all conditions must be satisfied
            foreach (var req in segment.UnlockCondition.Requirements)
            {
                if (tracker.GetCompleted(req.RequiredType) < req.RequiredCount)
                    return false;
            }
            return true;
        }
        else
        {
            // OR logic: any condition satisfied is enough
            foreach (var req in segment.UnlockCondition.Requirements)
            {
                if (tracker.GetCompleted(req.RequiredType) >= req.RequiredCount)
                    return true;
            }
            return false;
        }
    }
}
