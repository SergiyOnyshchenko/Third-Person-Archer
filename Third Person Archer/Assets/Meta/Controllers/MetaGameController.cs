using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;

public class MetaGameController : MonoBehaviour
{
    [SerializeField] private MissionProgressData _progressData;

    private MissionUnlockNotifier _unlockNotifier;
    public ZoneData CurrentZone => _progressData.Zone;
    public MissionType SelectedMissionType => _progressData.MissionType;
    public MissionUnlockNotifier UnlockNotifier => _unlockNotifier;
    public MissionProgressData MissionProgress => _progressData;

    private void Start()
    {
        InitUnlockNotifier();
        InjectChilds();
        TryUnlockNextZone();
    }

    public void SelectZone(int index)
    {
        _progressData.SelectZone(index);
        InitUnlockNotifier();
    }

    public void SelectMission(MissionType missionType)
    {
        _unlockNotifier?.MarkSeen(missionType);
    }

    public bool CanPlaySelectedMission()
    {
        var mission = _progressData.GetMission(SelectedMissionType);
        var segment = _progressData.GetSegment(SelectedMissionType);

        Debug.Log("Mission Name " + mission.name + " " + MissionUnlockService.CanUnlock(mission, _progressData.Zone));

        return MissionUnlockService.CanUnlock(mission, _progressData.Zone);
    }

    public void PlaySelectedMission()
    {
        if (!CanPlaySelectedMission())
        {
            Debug.LogWarning("Attempted to play a locked or invalid mission.");
            PopupManager.Instance.EnqueuePopup(PopupType.Error, "This mission is locked. Complete previous missions first.");
            return;
        }

        var mission = _progressData.GetMission(SelectedMissionType);
        if (mission == null)
        {
            Debug.LogError("No mission found for selected type.");
            return;
        }

        ScenesLoader.Instance.LoadCurrentMission();
    }

    public void CompleteCurrentMission()
    {
        if (_progressData.Zone != null)
        {
            _progressData.Zone.AdvanceMission(SelectedMissionType);
        }
        else
        {
            Debug.LogWarning("Zone is null. Cannot advance mission.");
        }

        _unlockNotifier.CheckForNewUnlocks();
    }

    public void TryUnlockNextZone()
    {
        _progressData.EvaluateZoneUnlocks();
    }

    private int GetCurrentZoneIndex()
    {
        for (int i = 0; i < _progressData.AllZones.Count; i++)
        {
            if (_progressData.Zone == _progressData.AllZones[i])
                return i;
        }
        return 0;
    }

    private void InitUnlockNotifier()
    {
        _unlockNotifier = new MissionUnlockNotifier(_progressData.Zone);
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

    private void InjectChilds()
    {
        var childs = GetComponentsInChildren<IMetaGameInjectable>(true);

        foreach (var child in childs)
            child.InjectMetaGameController(this);
    }
}