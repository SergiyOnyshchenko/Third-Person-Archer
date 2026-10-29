using UnityEngine;

public static class VictoryContextFactory
{
    public static VictoryContext FromMissionContext(
        MissionContext ctx,
        MissionProgressData progress)
    {
        if (ctx == null || !ctx.IsValid || progress == null)
            return null;

        int indexInsideZone = 0;

        if (ctx.SelectedType == MissionType.Campaign)
        {
            // Position of the played level (not the segment cursor: it has already moved / wraps in Heroic).
            var missions = ctx.Zone.GetSegmentByType(MissionType.Campaign)?.Missions;
            if (missions != null)
                for (int i = 0; i < missions.Count; i++)
                    if (missions[i] == ctx.Mission)
                        indexInsideZone = i;
        }

        int enemies = 0;

        if(EnemyManager.Instance != null)
            enemies = EnemyManager.Instance.GetDeadEnemiesCount();

        return new VictoryContext
        {
            missionType = ctx.SelectedType,
            zoneIndex = ctx.ZoneIndex,
            indexInsideZone = indexInsideZone,
            isHeroic = ctx.IsHeroic,

            enemiesKilled = enemies
        };
    }
}