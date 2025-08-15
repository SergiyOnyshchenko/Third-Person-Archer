using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ZoneData", menuName = "GameMeta/ZoneData", order = 1)]
public class ZoneData : ScriptableObject
{

    #region Serialized Fields

    [Header("Zone Info")]
    [SerializeField] private string _zoneName;
    [SerializeField] private string _id;
    [SerializeField] private UnlockCondition _unlockCondition;

    [Header("Mission Data")]
    [SerializeField] private List<MissionSegmentData> _segments = new();

    #endregion

    #region Public Properties

    public string Id => _id;
    public string Name => _zoneName;
    public float BossProgress
    {
        get
        {
            var bossSegment = GetSegmentByType(MissionType.Boss);
            if (bossSegment == null || bossSegment.Missions.Count == 0)
                return 0f;

            var bossMission = bossSegment.Missions[0];
            if (bossMission.UnlockCondition == null || bossMission.UnlockCondition.Requirements.Count == 0)
                return 1f;

            return MissionUnlockService.GetUnlockProgress(bossMission, this);
        }
    }

    public IReadOnlyList<MissionSegmentData> Segments => _segments;
    public UnlockCondition UnlockCondition => _unlockCondition;

    #endregion

    #region Initialization

    public void Init()
    {
        foreach (var segment in _segments)
        {
            string key = _zoneName + segment.Type + "Index";
            segment.Init(key);
        }
    }

    #endregion

    #region Mission Access

    public MissionSegmentData GetSegmentByType(MissionType type)
    {
        return _segments.Find(s => s.Type == type);
    }

    public MissionData GetMission(MissionType type)
    {
        return GetSegmentByType(type)?.GetCurrentMission();
    }

    public void AdvanceMission(MissionType type)
    {
        GetSegmentByType(type)?.Advance();
    }

    #endregion

    #region Zone Progress

    public bool IsBossUnlocked()
    {
        var bossSegment = GetSegmentByType(MissionType.Boss);
        var bossMission = bossSegment?.GetFirstMission();
        return MissionUnlockService.CanUnlock(bossMission, this);
    }

    public int GetCompletedCount(MissionType type)
    {
        return GetSegmentByType(type)?.GetCompletedCount() ?? 0;
    }

    public int GetTotalCompletedCount()
    {
        int total = 0;
        foreach (var segment in _segments)
            total += segment.GetCompletedCount();

        if (IsBossUnlocked())
            total += 1;

        return total;
    }

    #endregion
}