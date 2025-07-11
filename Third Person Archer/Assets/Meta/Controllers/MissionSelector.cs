using System;

public class MissionSelector
{
    private MissionType _selectedMission = MissionType.Campaign;

    public event Action<MissionType> OnMissionSelected;

    public MissionType Selected => _selectedMission;

    public void Select(MissionType missionType)
    {
        if (_selectedMission != missionType)
        {
            _selectedMission = missionType;
            OnMissionSelected?.Invoke(_selectedMission);
        }
    }
}
