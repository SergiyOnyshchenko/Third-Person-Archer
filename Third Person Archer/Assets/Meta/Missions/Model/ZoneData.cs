using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ZoneData", menuName = "GameMeta/ZoneData", order = 1)]
public class ZoneData : ScriptableObject
{
    [Header("Zone Info")]
    [Tooltip("Stable unique zone id used for saving. Do not change once shipped.")]
    [SerializeField] private string _id;

    [Tooltip("Name shown in UI.")]
    [SerializeField] private string _zoneName;

    [Header("Segments")]
    [Tooltip("Mission segments (Campaign, Contracts, Sniper, Boss). Campaign is the primary segment for zone completion.")]
    [SerializeField] private List<MissionSegmentData> _segments = new();

    public string ID => _id;
    public string ZoneName => _zoneName;
    public IReadOnlyList<MissionSegmentData> Segments => _segments;

    public MissionSegmentData GetSegmentByType(MissionType type)
    {
        return _segments.Find(s => s.Type == type);
    }

    public MissionData GetMission(MissionType type)
    {
        return GetSegmentByType(type)?.GetCurrentMission();
    }

    public void Init()
    {
        // Use stable zone id for save keys (NOT zone name).
        string zoneKey = string.IsNullOrWhiteSpace(_id) ? name : _id;

        foreach (var segment in _segments)
        {
            string key = zoneKey + "_" + segment.Type;
            segment.Init(key);
        }
    }

    // --------- New system helpers ---------

    public bool HasBoss()
    {
        var boss = GetSegmentByType(MissionType.Boss);
        return boss != null && boss.Missions != null && boss.Missions.Count > 0;
    }

    public bool IsCampaignComplete()
    {
        var campaign = GetSegmentByType(MissionType.Campaign);
        return campaign == null || campaign.IsFullyCompleted();
    }

    public bool IsBossCompleted()
    {
        if (!HasBoss())
            return true;

        // Boss segment: treat "completed once" of first mission as boss completed.
        var boss = GetSegmentByType(MissionType.Boss);
        if (boss == null || boss.Missions.Count == 0)
            return true;

        return boss.IsMissionCompletedOnce(boss.Missions[0]);
    }

    public bool IsBossUnlocked()
    {
        // Boss unlock rule: all campaign missions completed in this zone.
        if (!HasBoss())
            return false;

        return IsCampaignComplete();
    }

    /// <summary>
    /// Progress to boss unlock (0..1). Used by BossProgressView slider.
    /// </summary>
    public float BossProgress01
    {
        get
        {
            var campaign = GetSegmentByType(MissionType.Campaign);
            if (campaign == null) return 1f;

            int total = campaign.Missions != null ? campaign.Missions.Count : 0;
            if (total <= 0) return 1f;

            int done = campaign.GetCompletedOnceCount();
            return Mathf.Clamp01(done / (float)total);
        }
    }

    /// <summary>
    /// Advances the segment (used by MissionCompletionService).
    /// </summary>
    public void AdvanceMission(MissionType type)
    {
        GetSegmentByType(type)?.Advance();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(_id))
            _id = System.Guid.NewGuid().ToString("N");

        if (string.IsNullOrWhiteSpace(_zoneName))
            _zoneName = name;
    }
#endif
}