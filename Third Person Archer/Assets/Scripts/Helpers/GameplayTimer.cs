using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameplayTimer : MonoBehaviour
{
    private bool _isTimerActive;
    public float Timer { get; private set; }

    public static GameplayTimer Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void OnEnable()
    {
        LevelEventSystem.OnLevelStarted.AddListener(StartTimer);
        LevelEventSystem.OnLevelFinished.AddListener(FinishTimer);
    }

    private void OnDisable()
    {
        LevelEventSystem.OnLevelStarted.RemoveListener(StartTimer);
        LevelEventSystem.OnLevelFinished.RemoveListener(FinishTimer);
    }

    public void StartTimer()
    {
        _isTimerActive = true;
    }

    public void FinishTimer()
    {
        _isTimerActive = false;
    }

    private void Update()
    {
        if (_isTimerActive)
        {
            Timer += Time.unscaledDeltaTime;
        }
    }
}
