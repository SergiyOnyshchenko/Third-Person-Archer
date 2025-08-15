//using System.Diagnostics;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class MissionUnlockService
{
    public static bool CanUnlock(MissionData mission, ZoneData zone)
    {
        if (mission == null) return false;

        UnlockCondition condition = mission.UnlockCondition;

        // No condition means it's always unlocked
        if (condition == null || condition.Requirements.Count == 0)
            return true;

        int fulfilled = 0;

        foreach (var requirement in condition.Requirements)
        {
            var segment = zone.GetSegmentByType(requirement.RequiredType);
            if (segment == null) continue;

            int completed = segment.GetCompletedCount();
            if (completed >= requirement.RequiredCount)
            {
                fulfilled++;
            }
        }

        switch (condition.LogicType)
        {
            case UnlockLogicType.All:
                return fulfilled == condition.Requirements.Count;

            case UnlockLogicType.Any:
                return fulfilled > 0;

            default:
                Debug.LogWarning($"Unknown unlock logic type: {condition.LogicType}");
                return false;
        }
    }

    public static List<UnlockRequirementProgress> GetUnlockProgressBreakdown(MissionData mission, ZoneData zone)
    {
        var result = new List<UnlockRequirementProgress>();

        if (mission?.UnlockCondition?.Requirements == null)
            return result;

        foreach (var req in mission.UnlockCondition.Requirements)
        {
            var segment = zone.GetSegmentByType(req.RequiredType);
            int completed = segment?.GetCompletedCount() ?? 0;

            result.Add(new UnlockRequirementProgress(req.RequiredType, completed, req.RequiredCount));
        }

        return result;
    }

    public static float GetUnlockProgress(MissionData mission, ZoneData zone)
    {
        var breakdown = GetUnlockProgressBreakdown(mission, zone);
        if (breakdown.Count == 0) return 1f;

        float totalProgress = 0f;

        foreach (var entry in breakdown)
            totalProgress += entry.Progress;

        return Mathf.Clamp01(totalProgress / breakdown.Count);
    }
    
    public static string GetLockedReason(MissionType type, ZoneData zone)
    {
        switch (type)
        {
            case MissionType.Campaign:
                return "Complete Contracts or Sniper missions to unlock new Campaign missions.";
            case MissionType.Boss:
                return "Complete all Campaign missions to unlock the Boss mission.";
            default:
                return "Complete previous missions to unlock this mission type.";
        }
    }
}
