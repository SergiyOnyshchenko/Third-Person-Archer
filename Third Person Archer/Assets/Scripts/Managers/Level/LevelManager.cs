using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private LevelDatabase _database;
    public LevelData CurrentLevel => _database.CurrentLevel;
    public static LevelManager Instance;

    private void Awake()
    {
        if(Instance == null)
            Instance = this;
        else
            Destroy(this);

        _database.Load();

        Application.targetFrameRate = 60;
    }

    private void OnEnable()
    {
        LevelEventSystem.OnLoadLevel.AddListener(LoadLevel);
        LevelEventSystem.OnLoadNextLevel.AddListener(LoadNextLevel);
        LevelEventSystem.OnReloadLevel.AddListener(ReloadLevel);
        LevelEventSystem.OnLoadMainMenu.AddListener(LoadMainMenu);
    }

    private void OnDisable()
    {
        LevelEventSystem.OnLoadLevel.RemoveListener(LoadLevel);
        LevelEventSystem.OnLoadNextLevel.RemoveListener(LoadNextLevel);
        LevelEventSystem.OnReloadLevel.RemoveListener(ReloadLevel);
        LevelEventSystem.OnLoadMainMenu.RemoveListener(LoadMainMenu);
    }

    public void LoadLevel(int index)
    {
        _database.TrySetLevel(index);
        LoadLevel(_database.CurrentLevel);
    }

    public void LoadNextLevel()
    {
        _database.SetNextLevel();
        LoadLevel(_database.CurrentLevel);
    }

    public void ReloadLevel()
    {
        LoadLevel(_database.CurrentLevel);
    }

    public void LoadMainMenu()
    {
        LoadLevel(_database.MainMenu);
    }

    private void LoadLevel(LevelData data)
    {
        if (Preloader.Instance == null)
        {
            SceneManager.LoadScene(data.Scene);
        }
        else
        {
            Preloader.Instance.FadeIn(() => SceneManager.LoadScene(data.Scene));
        }
    }
}
