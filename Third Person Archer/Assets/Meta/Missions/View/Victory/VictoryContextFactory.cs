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
            var zone = progress.Zone;
            var segment = zone != null
                ? zone.GetSegmentByType(MissionType.Campaign)
                : null;

            if (segment != null)
                indexInsideZone = Mathf.Max(0, segment.CurrentIndex - 1);
        }

        return new VictoryContext
        {
            missionType = ctx.SelectedType,
            zoneIndex = ctx.ZoneIndex,
            indexInsideZone = indexInsideZone,
            loopIndex = ctx.LoopIndex,

            // As requested: animation test value
            enemiesKilled = 10
        };
    }
}