public static class ZoneUnlockService 
{
    public static bool CanUnlockZone(ZoneProgressData progressData, int zoneIndex)
    {
        if (progressData == null || zoneIndex <= 0 || zoneIndex >= progressData.AllZones.Count)
            return false;

        ZoneData previousZone = progressData.AllZones[zoneIndex - 1];
        return previousZone.IsBossUnlocked();
    }
}
