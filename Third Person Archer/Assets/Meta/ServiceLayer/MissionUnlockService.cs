public static class MissionUnlockService
{
    public static bool CanUnlock(MissionData mission, ZoneData zone)
    {
        if (mission == null || zone == null)
            return false;

        switch (mission.MissionType)
        {
            case MissionType.Campaign:
                return true;

            case MissionType.Contracts:
                return zone.GetMission(MissionType.Campaign)?.IsCompleted ?? false;

            case MissionType.Sniper:
                return zone.GetMission(MissionType.Contracts)?.IsCompleted ?? false;

            case MissionType.Boss:
                return zone.IsBossUnlocked();

            default:
                return false;
        }
    }
}
