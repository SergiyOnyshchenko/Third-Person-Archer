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

    /// <summary>Current mission of the segment; Campaign is the Heroic level once the whole campaign is done.</summary>
    public MissionData GetMission(MissionType type) =>
        type == MissionType.Campaign && IsHeroicUnlocked()
            ? GetHeroicCurrentMission(Zone)
            : GetSegment(type)?.GetCurrentMission();

    /// <summary>Loaded instance (set by DataManager); null in edit mode tools.</summary>
    public static MissionProgressData Active { get; private set; }

    public override void Initialize()
    {
        Active = this;

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

    /// <summary>Completed campaign missions across ALL zones at least once (Sniper levels on the path don't count).</summary>
    public int GetCompletedGlobalCampaignCount()
    {
        if (_zones == null) return 0;

        int total = 0;
        foreach (var z in _zones)
        {
            var campaign = z?.GetSegmentByType(MissionType.Campaign);
            if (campaign?.Missions == null) continue;

            foreach (var m in campaign.Missions)
                if (m != null && !m.IsSniper && campaign.IsMissionCompletedOnce(m))
                    total++;
        }

        return total;
    }

    /// <summary>Index of a Sniper level among all Sniper levels of the path (zone order); -1 if not a Sniper level.</summary>
    public int GetGlobalSniperIndex(MissionData mission)
    {
        if (_zones == null || mission == null || !mission.IsSniper) return -1;

        int index = 0;
        foreach (var z in _zones)
        {
            var missions = z?.GetSegmentByType(MissionType.Campaign)?.Missions;
            if (missions == null) continue;

            foreach (var m in missions)
            {
                if (m == null || !m.IsSniper) continue;
                if (m == mission) return index;
                index++;
            }
        }

        return -1;
    }

    public int GetSniperLevelCount()
    {
        int count = 0;
        if (_zones == null) return 0;

        foreach (var z in _zones)
        {
            var missions = z?.GetSegmentByType(MissionType.Campaign)?.Missions;
            if (missions == null) continue;
            foreach (var m in missions)
                if (m != null && m.IsSniper) count++;
        }

        return count;
    }

    /// <summary>
    /// Last cleared path level of the class, for a replay (gold + essence of that class): current zone
    /// first, then earlier zones, then later ones. Null if no such level is cleared yet.
    /// </summary>
    public bool TryFindReplayLevel(Meta.Weapons.WeaponClass weaponClass, out MissionData mission, out int zoneIndex)
    {
        mission = null;
        zoneIndex = -1;
        if (_zones == null) return false;

        for (int step = 0; step < _zones.Count; step++)
        {
            int zi = step <= _zoneIndex ? _zoneIndex - step : step;
            if (zi < 0 || zi >= _zones.Count) continue;

            var campaign = _zones[zi]?.GetSegmentByType(MissionType.Campaign);
            if (campaign?.Missions == null) continue;

            for (int i = campaign.Missions.Count - 1; i >= 0; i--)
            {
                var m = campaign.Missions[i];
                if (m == null || !campaign.IsMissionCompletedOnce(m)) continue;

                var cls = m.UseFixedWeaponClass ? m.FixedWeaponClass : m.BaseWeaponClass;
                if (cls != weaponClass) continue;

                mission = m;
                zoneIndex = zi;
                return true;
            }
        }

        return false;
    }

    /// <summary>Company level = completed global campaign count + 1.</summary>
    public int GetCompanyLevel()
    {
        return GetCompletedGlobalCampaignCount() + 1;
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

    /// <summary>
    /// Zone the menu opens on. Before Heroic only the last unlocked zone is playable; after the
    /// campaign every zone is open (Heroic levels, boss replays), so keep the player's choice.
    /// </summary>
    public int GetMenuZoneIndex()
    {
        return IsHeroicUnlocked() ? _zoneIndex : GetLastUnlockedZoneIndex();
    }

    /// <summary>Normal Campaign done in this zone (normal mode) or Heroic level cleared (Heroic mode).</summary>
    public bool IsCampaignLevelCleared(ZoneData zone, MissionData mission)
    {
        if (zone == null || mission == null) return false;
        if (IsHeroicUnlocked()) return IsHeroicLevelCleared(zone, mission);

        var campaign = zone.GetSegmentByType(MissionType.Campaign);
        return campaign != null && campaign.IsMissionCompletedOnce(mission);
    }

    // --------- Heroic mode (replaces campaign loops) ---------
    // Opens when every zone's Campaign is complete. Each zone has its own linear Heroic progress;
    // after the last level it starts over (replays keep the Heroic rewards).

    public bool IsHeroicUnlocked()
    {
        if (_zones == null || _zones.Count == 0) return false;

        foreach (var z in _zones)
            if (z == null || !z.IsCampaignComplete())
                return false;

        return true;
    }

    /// <summary>Heroic clears in the zone (monotonic; >= level count means the zone was cleared once).</summary>
    public int GetHeroicCompleted(ZoneData zone) =>
        zone != null ? SaveSystem.Load(HeroicKey(zone), 0) : 0;

    public int GetHeroicLevelCount(ZoneData zone)
    {
        var missions = zone?.GetSegmentByType(MissionType.Campaign)?.Missions;
        return missions != null ? missions.Count : 0;
    }

    public bool IsHeroicZoneCleared(ZoneData zone)
    {
        int count = GetHeroicLevelCount(zone);
        return count > 0 && GetHeroicCompleted(zone) >= count;
    }

    public int GetHeroicCurrentIndex(ZoneData zone)
    {
        int count = GetHeroicLevelCount(zone);
        return count > 0 ? GetHeroicCompleted(zone) % count : 0;
    }

    public MissionData GetHeroicCurrentMission(ZoneData zone)
    {
        var missions = zone?.GetSegmentByType(MissionType.Campaign)?.Missions;
        if (missions == null || missions.Count == 0) return null;
        return missions[GetHeroicCurrentIndex(zone)];
    }

    public bool IsHeroicLevelCleared(ZoneData zone, MissionData mission)
    {
        var missions = zone?.GetSegmentByType(MissionType.Campaign)?.Missions;
        if (missions == null || mission == null) return false;

        int idx = IndexOfMission(missions, mission);
        if (idx < 0) return false;

        int done = GetHeroicCompleted(zone);
        return done >= missions.Count || idx < done;
    }

    public void AdvanceHeroic(ZoneData zone)
    {
        if (zone == null) return;
        SaveSystem.Save(HeroicKey(zone), GetHeroicCompleted(zone) + 1);
    }

    /// <summary>
    /// k / n over the WHOLE campaign: k = same-class levels before this one, n = same-class levels.
    /// Heroic R grows across zones on one tier (Zone 1 Heroic is the easiest, Zone 3 the hardest).
    /// </summary>
    public float GetHeroicPosition01(MissionData mission, Meta.Weapons.WeaponClass weaponClass)
    {
        if (_zones == null || mission == null) return 0f;

        int before = 0, total = 0;
        bool found = false;

        foreach (var z in _zones)
        {
            var missions = z?.GetSegmentByType(MissionType.Campaign)?.Missions;
            if (missions == null) continue;

            foreach (var m in missions)
            {
                if (m == null) continue;
                if (m == mission) found = true;

                var cls = m.UseFixedWeaponClass ? m.FixedWeaponClass : m.BaseWeaponClass;
                if (cls != weaponClass) continue;

                total++;
                if (!found) before++;
            }
        }

        return found && total > 0 ? (float)before / total : 0f;
    }

    /// <summary>Index of the mission among all missions of this type across zones (in zone order); -1 if absent.</summary>
    public int GetGlobalSegmentIndex(MissionType type, MissionData mission)
    {
        if (_zones == null || mission == null) return -1;

        int offset = 0;
        foreach (var z in _zones)
        {
            var missions = z?.GetSegmentByType(type)?.Missions;
            if (missions == null) continue;

            int idx = IndexOfMission(missions, mission);
            if (idx >= 0) return offset + idx;
            offset += missions.Count;
        }

        return -1;
    }

    private static int IndexOfMission(IReadOnlyList<MissionData> missions, MissionData mission)
    {
        for (int i = 0; i < missions.Count; i++)
            if (missions[i] == mission) return i;
        return -1;
    }

    private static string HeroicKey(ZoneData zone) =>
        "heroic_" + (string.IsNullOrWhiteSpace(zone.ID) ? zone.name : zone.ID) + "_done";

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