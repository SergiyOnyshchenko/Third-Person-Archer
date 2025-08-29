using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;

public static class ZoneUnlockService
{
    public static bool CanUnlock(ZoneData zoneToUnlock, ZoneData previousZone)
    {
        if (zoneToUnlock == null || previousZone == null)
            return false;

        var condition = zoneToUnlock.UnlockCondition;
        if (condition == null || condition.Requirements == null || condition.Requirements.Count == 0)
            return false;

        int satisfied = 0;
        foreach (var req in condition.Requirements)
        {
            int completed = previousZone.GetSegmentByType(req.RequiredType)?.GetCompletedCount() ?? 0;
            if (completed >= req.RequiredCount)
                satisfied++;
        }

        return satisfied == condition.Requirements.Count;
    }
}