using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ZoneData", menuName = "GameMeta/ZoneData", order = 1)]
public class ZoneData : ScriptableObject
{
    [Header("Zone Info")]
    [SerializeField] private string _zoneName;
    [SerializeField] private bool _isZoneUnlocked;

    [Header("Mission Data")]
    [SerializeField] private List<MissionData> _allMissions;
    [Space]
    [SerializeField] private List<MissionSegmentData> _segments = new List<MissionSegmentData>();
    [SerializeField] private MissionData _bossMission;

    [Header("Boss Unlock Progress")]
    [Range(0, 1)]
    [SerializeField] private float _bossUnlockProgress;
    
    public string ZoneName => _zoneName;
    public bool IsZoneUnlocked => _isZoneUnlocked;
    public float BossUnlockProgress => _bossUnlockProgress;

    public void Init()
    {
        _segments = new List<MissionSegmentData>
        {
            CreateSegment(MissionType.Campaign),
            CreateSegment(MissionType.Sniper),
            CreateSegment(MissionType.Contracts)
        };

        var bossMission = GetMissionsByType(MissionType.Boss);

        if (bossMission != null && bossMission.Count > 0)
            _bossMission = bossMission[0];
    }

    public bool IsBossUnlocked()
    {
        return _bossUnlockProgress >= 1f;
    }

    public void IncreaseBossProgress(float value)
    {
        if (!IsBossUnlocked())
        {
            _bossUnlockProgress = Mathf.Clamp01(_bossUnlockProgress + value);
        }
    }

    public MissionData GetMission(MissionType type)
    {
        if (type == MissionType.Boss)
        {
            return _bossMission;
        }
        else
        {
            foreach (var segment in _segments)
                if (segment.Type == type)
                    return segment.GetCurrentMission();
        }

        return null;
    }

    public void UnlockZone()
    {
        _isZoneUnlocked = true;
    }

    private MissionSegmentData CreateSegment(MissionType type)
    {
        return new MissionSegmentData(
            type,
            GetMissionsByType(type),
            _zoneName + type.ToString() + "Index");
    }

    private List<MissionData> GetMissionsByType(MissionType type)
    {
        return _allMissions.FindAll(m => m.MissionType == type);
    }
}
