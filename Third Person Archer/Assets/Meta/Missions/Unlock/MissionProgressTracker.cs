using System.Collections.Generic;

public class MissionProgressTracker
{
    private Dictionary<MissionType, int> _completedByType = new();

    public MissionProgressTracker(ZoneData zone)
    {
        foreach (MissionType type in System.Enum.GetValues(typeof(MissionType)))
        {
            _completedByType[type] = zone.GetCompletedCount(type);
        }
    }

    public int GetCompleted(MissionType type) => _completedByType[type];
}