using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[System.Serializable]
public class MissionSegmentData
{
    [SerializeField] private MissionType _type;
    [SerializeField] private List<MissionData> _missions = new List<MissionData>();
    private string _saveKey;
    private int _index = 0;
    public int Index { get { return Mathf.Clamp(_index, 0, _missions.Count - 1); } }
    public MissionType Type { get => _type; }

    public MissionSegmentData(MissionType type, List<MissionData> missions, string saveKey)
    {
        _type = type;
        _missions = missions;
        _saveKey = saveKey;

        Load();
    }

    public MissionData GetCurrentMission()
    {
        return _missions[Index];
    }

    public void Load()
    {
        _index = PlayerPrefs.GetInt(_saveKey, 0);
    }

    public void Save()
    {
        PlayerPrefs.SetInt(_saveKey, _index);
    }
}
