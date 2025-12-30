using System.Collections.Generic;

public sealed class MissionProgressWriteAdapter : IMetaProgressWrite
{
    private readonly MissionProgressData _data;

    public MissionProgressWriteAdapter(MissionProgressData data)
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

    public void SelectZone(int index)
    {
        if (_data == null) return;
        _data.SelectZone(index);
    }

    public void SelectMissionType(MissionType type)
    {
        if (_data == null) return;
        _data.SelectMissionType(type);
    }
}