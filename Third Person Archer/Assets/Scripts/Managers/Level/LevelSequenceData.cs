using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelSequenceData", menuName = "Data/Level/LevelSequenceData")]
public class LevelSequenceData : ScriptableObject
{
    [SerializeField] private LevelData[] _levels;
    [SerializeField] private LevelData _bossLevel;
    public LevelData[] Levels => GetAllLevels();
    public bool HasBossLevel => CheckBossLevel();
    public LevelData BossLevel { get => _bossLevel;}

    private LevelData[] GetAllLevels()
    {
        List<LevelData> levels = new List<LevelData>(_levels);

        if(_bossLevel != null)
            levels.Add(_bossLevel);

        return levels.ToArray();
    }

    private bool CheckBossLevel()
    {
        if (_bossLevel != null)
            return true;
        else
            return false;
    }
}
