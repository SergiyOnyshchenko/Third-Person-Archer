using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[System.Serializable]
public class MissionSegmentData
{

    #region Serialized Fields

    [SerializeField] private MissionType _type;
    [SerializeField] private List<MissionData> _missions = new();

    #endregion

    #region Private Runtime Data

    private string _saveKey;
    private int _index = 0;
    private int _realIndex = 0;

    #endregion

    #region Public Properties

    public MissionType Type => _type;
    public int Index => Mathf.Clamp(_index, 0, _missions.Count - 1);
    public int RealIndex => _realIndex;
    public IReadOnlyList<MissionData> Missions => _missions;

    #endregion

    #region Initialization

    public void Init(string saveKey)
    {
        _saveKey = saveKey;
        Load();
    }

    #endregion

    #region Mission Navigation

    public MissionData GetCurrentMission()
    {
        return _missions.Count > 0 ? _missions[Index] : null;
    }

    public MissionData GetFirstPlayableMission()
    {
        for (int i = 0; i < _missions.Count; i++)
        {
            bool isUnlocked = i <= _index;
            bool isCompleted = i < _realIndex;

            if (isUnlocked && !isCompleted)
                return _missions[i];
        }

        return null;
    }

    public MissionData GetFirstMission()
    {
        return _missions.Count > 0 ? _missions[0] : null;
    }

    public int GetMissionIndex(MissionData mission)
    {
        return _missions.IndexOf(mission);
    }

    public void Advance()
    {
        _realIndex++;
        _index++;

        if (_index >= _missions.Count)
            _index = 0;

        Save();
    }


    public bool IsMissionUnlocked(MissionData mission)
    {
        return _missions.IndexOf(mission) <= _index;
    }

    public bool IsMissionCompleted(MissionData mission)
    {
        return _missions.IndexOf(mission) < _realIndex;
    }

    #endregion

    #region Progress & Saving

    public int GetCompletedCount()
    {
        return _realIndex;
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

    #endregion
}