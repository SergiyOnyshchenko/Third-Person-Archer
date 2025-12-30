using System.Collections.Generic;

public interface IMetaProgressReadOnly
{
    ZoneData CurrentZone { get; }
    MissionType SelectedMissionType { get; }
    IReadOnlyList<ZoneData> AllZones { get; }

    MissionData GetMission(MissionType type);
}