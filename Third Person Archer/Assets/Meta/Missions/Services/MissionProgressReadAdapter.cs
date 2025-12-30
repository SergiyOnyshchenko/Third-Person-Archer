using System.Collections.Generic;

public sealed class MissionProgressReadAdapter : IMetaProgressReadOnly
{
    private readonly MissionProgressData _data;

    public MissionProgressReadAdapter(MissionProgressData data)
    {
        _data = data;
    }

    public ZoneData CurrentZone => _data != null ? _data.Zone : null;

    public MissionType SelectedMissionType => _data != null ? _data.MissionType : MissionType.Campaign;

    public IReadOnlyList<ZoneData> AllZones => _data != null ? _data.AllZones : null;

    public MissionData GetMission(MissionType type)
    {
        return _data != null ? _data.GetMission(type) : null;
    }
}