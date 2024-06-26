using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelDatabase", menuName = "Data/Level/LevelDatabase")]
public class LevelDatabase : ScriptableObject
{
    [SerializeField] private LevelData _mainMenu;
    [SerializeField] private LevelData[] _levels;
    private int _currentLevelIndex;
    private string _saveName = "CurrentLevel";
    public LevelData CurrentLevel => _levels[_currentLevelIndex];
    public LevelData MainMenu { get => _mainMenu; }

    public void SetNextLevel()
    {
        _currentLevelIndex++;

        if(_currentLevelIndex >= _levels.Length)    
            _currentLevelIndex = 0;

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
        PlayerPrefs.SetInt(_saveName, _currentLevelIndex);
    }

    public void Load()
    {
        _currentLevelIndex = PlayerPrefs.GetInt(_saveName);
    }
}
