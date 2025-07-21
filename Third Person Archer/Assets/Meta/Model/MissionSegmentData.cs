using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class MissionSegmentData
{
    [SerializeField] private MissionType _type;
    [SerializeField] private List<MissionData> _missions = new List<MissionData>();
    [SerializeField] private UnlockCondition _unlockCondition;
    private string _saveKey;
    private int _index = 0;
    private int _realIndex = 0;

    public int Index => Mathf.Clamp(_index, 0, _missions.Count - 1);
    public int RealIndex => _realIndex;

    public MissionType Type => _type;
    public UnlockCondition UnlockCondition => _unlockCondition;

    public void Init(string saveKey)
    {
        _saveKey = saveKey;

        Load();
    }

    public void Advance()
    {
        _realIndex++;
        _index++;

        if (_index >= _missions.Count)
            _index = 0;

        Save();
    }

    public MissionData GetCurrentMission()
    {
        return _missions[Index];
    }

    public int GetCompletedCount()
    {
        return _missions.Count(m => m.IsCompleted);
    }

    public int GetMissionIndex(MissionData mission)
    {
        return _missions.IndexOf(mission);
    }

    public void Load()
    {
        _index = SaveSystem.Load(_saveKey + "_index", 0);
        _realIndex = SaveSystem.Load(_saveKey + "_real", 0);
    }

    public void Save()
    {
        SaveSystem.Save(_saveKey + "_index", _index);
        SaveSystem.Save(_saveKey + "_real", _realIndex);
    }
}