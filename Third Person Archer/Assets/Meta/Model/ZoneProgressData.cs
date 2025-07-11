using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "GameProgressData", menuName = "GameMeta/GameProgressData", order = 2)]
public class ZoneProgressData : ScriptableObject
{
    [SerializeField] private List<ZoneData> _allZones;
    [SerializeField] private int _currentZoneIndex = 0;

    public IReadOnlyList<ZoneData> AllZones => _allZones;
    public ZoneData CurrentZone => _allZones != null && _allZones.Count > _currentZoneIndex ? _allZones[_currentZoneIndex] : null;

    public void SelectZone(int index)
    {
        if (index >= 0 && index < _allZones.Count && _allZones[index].IsZoneUnlocked)
        {
            _currentZoneIndex = index;
        }
    }

    public void UnlockNextZone()
    {
        int nextIndex = _currentZoneIndex + 1;
        if (nextIndex < _allZones.Count)
        {
            _allZones[nextIndex].UnlockZone();
        }
    }

    public void ResetProgress()
    {
        foreach (var zone in _allZones)
        {
            // Implement reset logic as needed per zone/missions
        }
        _currentZoneIndex = 0;
    }
}
