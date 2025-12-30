using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "MissionProgressData", menuName = "GameMeta/MissionProgressData")]
public class MissionProgressData : GameData
{
    [Header("Zones (fixed order)")]
    [SerializeField] private List<ZoneData> _zones = new();

    [Header("Selection")]
    [SerializeField] private int _zoneIndex;
    [SerializeField] private MissionType _missionType = MissionType.Campaign;

    [Header("Progress (derived & saved)")]
    [Tooltip("Last unlocked zone index. Used by menu to force current map.")]
    [SerializeField] private int _lastUnlockedZoneIndex = 0;

    public UnityEvent OnZoneChanged = new UnityEvent();
    public UnityEvent OnMissionTypeChanged = new UnityEvent();

    public List<ZoneData> AllZones => _zones;

    public int ZoneIndex => _zoneIndex;
    public int LastUnlockedZoneIndex => _lastUnlockedZoneIndex;

    public MissionType MissionType => _missionType;

    public ZoneData Zone =>
        _zones != null && _zones.Count > 0 && _zoneIndex >= 0 && _zoneIndex < _zones.Count
            ? _zones[_zoneIndex]
            : null;

    public MissionSegmentData GetSegment(MissionType type) =>
        Zone?.Segments.FirstOrDefault(s => s.Type == type);

    public MissionData GetMission(MissionType type) =>
        GetSegment(type)?.GetCurrentMission();

    public override void Initialize()
    {
        foreach (var z in _zones)
            z?.Init();

        LoadPersistentData();
        RecalculateLastUnlockedZoneIndex();
    }

    // --------- Selection ---------

    public void SelectZone(int index)
    {
        if (_zones == null || _zones.Count == 0)
            return;

        index = Mathf.Clamp(index, 0, _zones.Count - 1);

        if (_zoneIndex == index)
            return;

        _zoneIndex = index;
        SaveSystem.Save("selected_zone_index", _zoneIndex);

        OnZoneChanged?.Invoke();
    }

    public void SelectMissionType(MissionType type)
    {
        if (_missionType == type)
            return;

        _missionType = type;
        SaveSystem.Save("selected_mission_type", (int)_missionType);

        OnMissionTypeChanged?.Invoke();
    }

    public int GetLastUnlockedZoneIndex()
    {
        RecalculateLastUnlockedZoneIndex();
        return _lastUnlockedZoneIndex;
    }

    public bool IsZoneUnlocked(ZoneData zone)
    {
        if (zone == null || _zones == null) return false;

        int idx = _zones.IndexOf(zone);
        return idx >= 0 && idx <= _lastUnlockedZoneIndex;
    }

    // --------- Global progress helpers ---------

    /// <summary>Completed campaign missions across ALL zones at least once.</summary>
    public int GetCompletedGlobalCampaignCount()
    {
        if (_zones == null) return 0;

        int total = 0;
        foreach (var z in _zones)
        {
            if (z == null) continue;

            var campaign = z.GetSegmentByType(MissionType.Campaign);
            if (campaign == null) continue;

            total += campaign.GetCompletedOnceCount();
        }

        return total;
    }

    /// <summary>Company level = completed global campaign count + 1.</summary>
    public int GetCompanyLevel()
    {
        return GetCompletedGlobalCampaignCount() + 1;
    }

    /// <summary>
    /// Contract pool: all campaign missions completed at least once across all zones (including previous zones).
    /// </summary>
    public IReadOnlyList<MissionData> GetEligibleContractCampaignMissions()
    {
        if (_zones == null) return System.Array.Empty<MissionData>();

        var result = new List<MissionData>(64);

        foreach (var z in _zones)
        {
            if (z == null) continue;

            var campaign = z.GetSegmentByType(MissionType.Campaign);
            if (campaign == null) continue;

            var missions = campaign.Missions;
            if (missions == null) continue;

            for (int i = 0; i < missions.Count; i++)
            {
                var m = missions[i];
                if (m == null) continue;

                if (campaign.IsMissionCompletedOnce(m))
                    result.Add(m);
            }
        }

        return result;
    }

    // --------- Unlock / zone progression (new rule) ---------

    /// <summary>
    /// Recalculates last unlocked zone based on progression rule:
    /// zone i is unlocked if zone i-1 is complete (boss completed if it has boss; otherwise campaign complete).
    /// </summary>
    public void RecalculateLastUnlockedZoneIndex()
    {
        if (_zones == null || _zones.Count == 0)
        {
            _lastUnlockedZoneIndex = 0;
            return;
        }

        int unlocked = 0;

        for (int i = 1; i < _zones.Count; i++)
        {
            var prev = _zones[i - 1];
            if (prev == null) break;

            bool prevComplete = prev.HasBoss()
                ? prev.IsBossCompleted()
                : prev.IsCampaignComplete();

            if (!prevComplete)
                break;

            unlocked = i;
        }

        if (_lastUnlockedZoneIndex != unlocked)
        {
            _lastUnlockedZoneIndex = unlocked;
            SaveSystem.Save("last_unlocked_zone_index", _lastUnlockedZoneIndex);
        }
    }

    // --------- Persistence ---------

    public void LoadPersistentData()
    {
        _zoneIndex = SaveSystem.Load("selected_zone_index", 0);
        _lastUnlockedZoneIndex = SaveSystem.Load("last_unlocked_zone_index", 0);
        _missionType = (MissionType)SaveSystem.Load("selected_mission_type", 0);

        // Clamp
        if (_zones != null && _zones.Count > 0)
            _zoneIndex = Mathf.Clamp(_zoneIndex, 0, _zones.Count - 1);
        else
            _zoneIndex = 0;
    }
}