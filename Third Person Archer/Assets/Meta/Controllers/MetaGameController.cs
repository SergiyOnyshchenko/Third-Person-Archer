using UnityEngine;

public class MetaGameController : MonoBehaviour
{
    [SerializeField] private ZoneProgressData _progressData;
    private MissionType _selectedMissionType = MissionType.Campaign;
    public ZoneData CurrentZone => _progressData.CurrentZone;
    public MissionType SelectedMissionType => _selectedMissionType;

    public void SelectZone(int index)
    {
        _progressData.SelectZone(index);
    }

    public void SelectMission(MissionType missionType)
    {
        _selectedMissionType = missionType;
    }

    public bool CanPlaySelectedMission()
    {
        //var mission = _progressData.CurrentZone?.GetMission(_selectedMissionType);
        //return MissionUnlockService.CanUnlock(mission, _progressData.CurrentZone);

        return true;
    }

    public void PlaySelectedMission()
    {
        Debug.Log($"Playing {_selectedMissionType} mission in zone {_progressData.CurrentZone.ZoneName} level {_progressData.CurrentZone.GetMission(_selectedMissionType).name}");
        // TODO: Load scene, trigger mission start
    }

    /*
    public void CompleteCampaignMission(BossProgressService bossService)
    {
        var campaign = _progressData.CurrentZone?.GetMission(MissionType.Campaign);
        if (campaign != null && !campaign.IsCompleted)
        {
            campaign.MarkCompleted();
            bossService.IncreaseProgress(_progressData.CurrentZone);
        }
    }
    */

    public void TryUnlockNextZone()
    {
        int currentIndex = GetCurrentZoneIndex();
        int nextIndex = currentIndex + 1;
        if (ZoneUnlockService.CanUnlockZone(_progressData, nextIndex))
        {
            _progressData.UnlockNextZone();
        }
    }

    private int GetCurrentZoneIndex()
    {
        for (int i = 0; i < _progressData.AllZones.Count; i++)
        {
            if (_progressData.CurrentZone == _progressData.AllZones[i])
                return i;
        }
        return 0;
    }
}
