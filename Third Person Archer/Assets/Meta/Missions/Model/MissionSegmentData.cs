using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MissionSegmentData
{
    [Header("Segment")]
    [Tooltip("Mission type this segment holds.")]
    [SerializeField] private MissionType _type;

    [Tooltip("Ordered list of missions for this segment.")]
    [SerializeField] private List<MissionData> _missions = new();

    // Runtime / persistent progress
    [SerializeField, Tooltip("Current mission index for this segment (resets on new loop/run if you choose).")]
    private int _currentIndex = 0;

    [SerializeField, Tooltip("Total number of completions in this segment across all time (monotonic).")]
    private int _totalCompletedCount = 0;

    [SerializeField, Tooltip("Per-mission flag: completed at least once.")]
    private List<bool> _completedOnce = new();

    private string _saveKey;

    public MissionType Type => _type;
    public IReadOnlyList<MissionData> Missions => _missions;

    public int CurrentIndex => _currentIndex;
    public int TotalCompletedCount => _totalCompletedCount;

    public void Init(string saveKey)
    {
        _saveKey = saveKey;
        EnsureCompletedOnceSize();
        Load();
    }

    public MissionData GetCurrentMission()
    {
        if (_missions == null || _missions.Count == 0)
            return null;

        int idx = Mathf.Clamp(_currentIndex, 0, _missions.Count - 1);
        return _missions[idx];
    }

    public bool IsFullyCompleted()
    {
        if (_missions == null) return true;
        return GetCompletedOnceCount() >= _missions.Count;
    }

    public int GetCompletedOnceCount()
    {
        EnsureCompletedOnceSize();

        int count = 0;
        for (int i = 0; i < _completedOnce.Count; i++)
            if (_completedOnce[i]) count++;

        return count;
    }

    public bool IsMissionCompletedOnce(MissionData mission)
    {
        if (mission == null || _missions == null) return false;

        EnsureCompletedOnceSize();
        int idx = _missions.IndexOf(mission);
        if (idx < 0 || idx >= _completedOnce.Count) return false;

        return _completedOnce[idx];
    }

    /// <summary>
    /// Marks the current mission as completed and advances index by 1 (clamped).
    /// Does NOT loop back automatically; loop logic is handled at higher level.
    /// </summary>
    public void Advance()
    {
        if (_missions == null || _missions.Count == 0)
            return;

        EnsureCompletedOnceSize();

        int idx = Mathf.Clamp(_currentIndex, 0, _missions.Count - 1);
        _completedOnce[idx] = true;

        _totalCompletedCount++;

        _currentIndex++;
        if (_currentIndex >= _missions.Count)
            _currentIndex = 0; 

        Save();
    }

    /// <summary>
    /// One-time save migration after Sniper missions moved onto the Campaign path. Old saves kept
    /// per-index flags of the Campaign-only list (same key) and of the old Sniper segment. Each
    /// old flag maps to the n-th Campaign / n-th Sniper mission of the new list (their order is kept).
    /// </summary>
    public void MigrateFromSplitSegments(string sniperSaveKey)
    {
        if (string.IsNullOrEmpty(_saveKey) || _missions == null) return;

        EnsureCompletedOnceSize();

        int campaign = 0, sniper = 0;
        for (int i = 0; i < _missions.Count; i++)
        {
            bool isSniper = _missions[i] != null && _missions[i].IsSniper;
            _completedOnce[i] = isSniper
                ? SaveSystem.Load(sniperSaveKey + "_once_" + sniper++, false)
                : SaveSystem.Load(_saveKey + "_once_" + campaign++, false);
        }

        // The path is linear: continue from the first level not cleared yet.
        _currentIndex = Mathf.Max(0, _completedOnce.IndexOf(false));
        Save();
    }

    private void EnsureCompletedOnceSize()
    {
        if (_missions == null)
        {
            _completedOnce.Clear();
            return;
        }

        if (_completedOnce == null)
            _completedOnce = new List<bool>();

        while (_completedOnce.Count < _missions.Count)
            _completedOnce.Add(false);

        while (_completedOnce.Count > _missions.Count)
            _completedOnce.RemoveAt(_completedOnce.Count - 1);
    }

    private void Load()
    {
        if (string.IsNullOrEmpty(_saveKey))
            return;

        _currentIndex = SaveSystem.Load(_saveKey + "_current", 0);
        _totalCompletedCount = SaveSystem.Load(_saveKey + "_total", 0);

        EnsureCompletedOnceSize();
        for (int i = 0; i < _completedOnce.Count; i++)
        {
            _completedOnce[i] = SaveSystem.Load(_saveKey + "_once_" + i, false);
        }
    }

    private void Save()
    {
        if (string.IsNullOrEmpty(_saveKey))
        {
            Debug.LogError($"[MissionSegmentData] SaveKey empty => Init() was not called. Type={_type}");
            return;
        }

        if (string.IsNullOrEmpty(_saveKey))
            return;

        SaveSystem.Save(_saveKey + "_current", _currentIndex);
        SaveSystem.Save(_saveKey + "_total", _totalCompletedCount);

        EnsureCompletedOnceSize();
        for (int i = 0; i < _completedOnce.Count; i++)
        {
            SaveSystem.Save(_saveKey + "_once_" + i, _completedOnce[i]);
        }
    }
}