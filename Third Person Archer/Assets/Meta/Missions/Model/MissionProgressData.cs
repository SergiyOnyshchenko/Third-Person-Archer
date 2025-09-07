using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "MissionProgressData", menuName = "GameMeta/MissionProgressData")]
public class MissionProgressData : GameData
{
    #region Serialized Fields

    [SerializeField] private List<ZoneData> _zones;
    [SerializeField] private int _zoneIndex;
    [SerializeField] private int _lastUnlockedZoneIndex = 0;
    [SerializeField] private MissionType _missionType;

    #endregion

    #region Public Properties

    public MissionData Mission { get; private set; }
    public MissionType MissionType => _missionType;
    public ZoneData Zone => _zones != null && _zones.Count > _zoneIndex ? _zones[_zoneIndex] : null;
    public List<ZoneData> AllZones => _zones;
    public MissionSegmentData GetSegment(MissionType type) => Zone?.Segments.FirstOrDefault(s => s.Type == type);
    public MissionData GetMission(MissionType type) => GetSegment(type)?.GetCurrentMission();

    #endregion

    public UnityEvent OnZoneChanged = new UnityEvent();
    public UnityEvent OnMissionTypeChanged = new UnityEvent();

    #region Selection Methods

    public void SelectZone(int index)
    {
        _zoneIndex = Mathf.Clamp(index, 0, _zones.Count - 1);

        Debug.Log($"Changing zone: {_zoneIndex}");

        SaveSystem.Save("selected_zone_index", _zoneIndex);
        ResolveMission();
        OnZoneChanged?.Invoke();
    }

    public void SelectMissionType(MissionType type)
    {
        _missionType = type;
        SaveSystem.Save("selected_mission_type", (int)_missionType);
        ResolveMission();
        OnMissionTypeChanged?.Invoke();
    }

    #endregion

    #region Mission Resolution

    private void ResolveMission()
    {
        if (Zone == null)
        {
            Mission = null;
            return;
        }

        var segment = Zone.GetSegmentByType(_missionType);
        Mission = segment?.GetFirstPlayableMission();
    }

    #endregion

    #region Zone Access

    public ZoneData GetZoneById(string id)
    {
        return _zones.Find(z => z.Id == id);
    }

    public IEnumerable<ZoneData> GetAllZones() => _zones;

    public bool IsZoneUnlocked(ZoneData zone)
    {
        return _zones.IndexOf(zone) <= _lastUnlockedZoneIndex;
    }

    public int GetCurrentZoneIndex()
    {
        return Mathf.Clamp(_zoneIndex, 0, AllZones.Count - 1);
    }

    public int GetLastUnlockedZoneIndex()
    {
        return Mathf.Clamp(_lastUnlockedZoneIndex, 0, AllZones.Count - 1);
    }

    #endregion

    #region Progress Evaluation

    public int GetTotalCompletedMissions()
    {
        int total = 0;
        foreach (var zone in _zones)
        {
            total += zone.GetTotalCompletedCount();
        }
        return total;
    }

    public void UnlockNextZone()
    {
        if (_lastUnlockedZoneIndex + 1 < _zones.Count)
        {
            _lastUnlockedZoneIndex++;
            SaveSystem.Save("last_unlocked_zone_index", _lastUnlockedZoneIndex);
        }
    }

    public void CompleteCurrentMission()
    {
        var segment = GetSegment(MissionType);
        segment.Advance();
    }

    public void EvaluateZoneUnlocks()
    {
        for (int i = 1; i < _zones.Count; i++)
        {
            var previousZone = _zones[i - 1];
            var currentZone = _zones[i];

            if (IsZoneUnlocked(currentZone)) continue;

            if (ZoneUnlockService.CanUnlock(currentZone, previousZone))
            {
                _lastUnlockedZoneIndex = i;
                SaveSystem.Save("last_unlocked_zone_index", _lastUnlockedZoneIndex);
  
                UI.Core.ServiceLocator.Resolve<UI.Core.IUINavigator>()
                    .ShowPopup("popup_unlock", new UI.Screens.ToastArgs{
                        Message = $"New zone unlocked: {currentZone.Name}",
                        Duration = 2f });

                Debug.Log($"Zone {currentZone.Id} unlocked.");
            }
        }
    }

    #endregion

    #region Initialization

    public override void Initialize()
    {
        LoadPersistentData();
    }

    public void LoadPersistentData()
    {
        _zoneIndex = SaveSystem.Load("selected_zone_index", 0);
        _lastUnlockedZoneIndex = SaveSystem.Load("last_unlocked_zone_index", 0);
        _missionType = (MissionType)SaveSystem.Load("selected_mission_type", 0);
        ResolveMission();
    }

    #endregion
}