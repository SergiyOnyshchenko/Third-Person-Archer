using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Data/Level/LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    [SerializeField] private LevelData _mainMenu;
    [SerializeField] private LevelData _preloadLevel;
    [SerializeField] private LevelData[] _levels;
    private int _currentLevelIndex;
    private int _loopCount;
    private string _saveNameCurrentLevel = "CurrentLevel";
    private string _saveNameLoopCount = "LoopCount";
    public LevelData CurrentLevel => _levels[_currentLevelIndex];
    public LevelData MainMenu { get => _mainMenu; }
    public LevelData PreloadLevel { get => _preloadLevel; }
    public int LevelNumber => LevelIndex * _loopCount + LevelIndex;
    public int LevelIndex => _currentLevelIndex;

    public void SetNextLevel()
    {
        _currentLevelIndex++;

        if(_currentLevelIndex >= _levels.Length)
        {
            _currentLevelIndex = 0;
            _loopCount++;
        }      

        Save();
    }

    public void TrySetLevel(int index)
    {
        index = Mathf.Clamp(index, 0, _levels.Length - 1);
        _currentLevelIndex = index;

        Save();
    }

    public void Save()
    {
        PlayerPrefs.SetInt(_saveNameCurrentLevel, _currentLevelIndex);
        PlayerPrefs.SetInt(_saveNameLoopCount, _loopCount);
    }

    public void Load()
    {
        _currentLevelIndex = PlayerPrefs.GetInt(_saveNameCurrentLevel);
        _loopCount = PlayerPrefs.GetInt(_saveNameLoopCount);
    }
}
