using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionUnlockNotifier
{
    private const string SaveKeyPrefix = "unlocked_seen_";
    private ZoneData _zone;
    private Dictionary<MissionType, bool> _newlyUnlocked = new();

    public event Action<MissionType> OnNewMissionUnlocked;

    public MissionUnlockNotifier(ZoneData zone)
    {
        _zone = zone;
        foreach (MissionType type in Enum.GetValues(typeof(MissionType)))
        {
            _newlyUnlocked[type] = false;
        }
    }

    public void CheckForNewUnlocks()
    {
        foreach (var segment in _zone.Segments)
        {
            var mission = segment.GetCurrentMission();

            if (mission == null || !mission.HasUnlockCondition())
                continue;

            if (MissionUnlockService.CanUnlock(mission, _zone))
            {
                string saveKey = GetSaveKey(_zone.Name, segment.Type);
                bool seen = SaveSystem.Load(saveKey, false);

                if (!seen && !_newlyUnlocked[segment.Type])
                {
                    _newlyUnlocked[segment.Type] = true;
                    OnNewMissionUnlocked?.Invoke(segment.Type);
                }
            }
        }
    }

    public bool IsNewlyUnlocked(MissionType type)
    {
        return _newlyUnlocked.ContainsKey(type) && _newlyUnlocked[type];
    }

    public void MarkSeen(MissionType type)
    {
        _newlyUnlocked[type] = false;
        SaveSystem.Save(GetSaveKey(_zone.Name, type), true);
    }

    public void ResetAllSeen()
    {
        foreach (var type in _newlyUnlocked.Keys.ToList())
        {
            _newlyUnlocked[type] = false;
            SaveSystem.Save(GetSaveKey(_zone.Name, type), false);
        }
    }

    private string GetSaveKey(string zoneName, MissionType type)
    {
        return SaveKeyPrefix + zoneName + "_" + type;
    }
}