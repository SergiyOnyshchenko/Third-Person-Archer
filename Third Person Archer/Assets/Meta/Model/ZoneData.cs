using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ZoneData", menuName = "GameMeta/ZoneData", order = 1)]
public class ZoneData : ScriptableObject
{
    [Header("Zone Info")]
    [SerializeField] private string _zoneName;
    [SerializeField] private bool _isZoneUnlocked;

    [Header("Mission Data")]
    [SerializeField] private List<MissionSegmentData> _segments = new List<MissionSegmentData>();
    [SerializeField] private MissionData _bossMission;

    [Header("Boss Unlock Progress")]
    [Range(0, 1)]
    [SerializeField] private float _bossUnlockProgress;

    public string ZoneName => _zoneName;
    public bool IsZoneUnlocked => _isZoneUnlocked;
    public float BossUnlockProgress => _bossUnlockProgress;
    public List<MissionSegmentData> Segments => _segments;

    public void Init()
    {
        foreach (var segment in _segments)
        {
            string key = _zoneName + segment.Type.ToString() + "Index";
            segment.Init(key);
        }
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

        foreach (var segment in _segments)
        {
            if (segment.Type == type)
                return segment.GetCurrentMission();
        }

        return null;
    }

    public void AdvanceMission(MissionType type)
    {
        var segment = _segments.Find(s => s.Type == type);
        if (segment != null)
        {
            segment.Advance();
        }
    }

    public void UnlockZone()
    {
        _isZoneUnlocked = true;
    }

    public int GetCompletedCount(MissionType type)
    {
        var segment = _segments.Find(s => s.Type == type);
        return segment?.GetCompletedCount() ?? 0;
    }

    public int GetTotalCompletedCount()
    {
        int total = 0;

        foreach (var segment in _segments)
            total += segment.GetCompletedCount();
            
        if (IsBossUnlocked()) total += 1;

        return total;
    }
}