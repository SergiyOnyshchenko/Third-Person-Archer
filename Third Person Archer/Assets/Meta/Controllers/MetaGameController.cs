using UnityEngine;

public class MetaGameController : MonoBehaviour
{
    [SerializeField] private ZoneProgressData _progressData;

    private MissionType _selectedMissionType = MissionType.Campaign;
    private MissionUnlockNotifier _unlockNotifier;

    public ZoneData CurrentZone => _progressData.CurrentZone;
    public MissionType SelectedMissionType => _selectedMissionType;
    public MissionUnlockNotifier UnlockNotifier => _unlockNotifier;

    private void Awake()
    {
        _progressData.InitAllZones();
        InitUnlockNotifier();
    }

    public void SelectZone(int index)
    {
        _progressData.SelectZone(index);
        InitUnlockNotifier();
    }

    public void SelectMission(MissionType missionType)
    {
        _selectedMissionType = missionType;
        _unlockNotifier?.MarkSeen(missionType);
    }

    public bool CanPlaySelectedMission()
    {
        var mission = _progressData.CurrentZone?.GetMission(_selectedMissionType);
        var segment = _progressData.CurrentZone?.Segments.Find(s => s.Type == _selectedMissionType);
        return MissionUnlockService.CanUnlock(mission, segment, _progressData.CurrentZone);
    }

    public void PlaySelectedMission()
    {
        Debug.Log($"Playing {_selectedMissionType} mission in zone {_progressData.CurrentZone.ZoneName} level {_progressData.CurrentZone.GetMission(_selectedMissionType).name}");

        // TODO: Load scene, trigger mission start
        // On success, call CompleteCurrentMission()
    }

    public void CompleteCurrentMission()
    {
        _progressData.CurrentZone.AdvanceMission(_selectedMissionType);
        _unlockNotifier.CheckForNewUnlocks();
    }

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

    private void InitUnlockNotifier()
    {
        _unlockNotifier = new MissionUnlockNotifier(_progressData.CurrentZone);
        _unlockNotifier.OnNewMissionUnlocked += HandleNewUnlock;
        _unlockNotifier.CheckForNewUnlocks();
    }

    private void HandleNewUnlock(MissionType type)
    {
        string message = type switch
        {
            MissionType.Campaign => "New Campaign mission unlocked!",
            MissionType.Sniper => "New Sniper mission unlocked!",
            MissionType.Contracts => "New Contract mission unlocked!",
            MissionType.Boss => "Boss mission unlocked!",
            _ => "New mission unlocked!"
        };

        PopupManager.Instance.EnqueuePopup(PopupType.Unlock, message);
    }
}