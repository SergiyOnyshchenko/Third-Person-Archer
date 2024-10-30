using UnityEngine;

public class LevelSDKEvents : MonoBehaviour
{
    [SerializeField] private bool _isLogEnabled;
    private bool _isLeaving = false;
    private void OnEnable()
    {
        SDK_EventSystem.OnLevelStarted.AddListener(LevelStartHandle);
        SDK_EventSystem.OnLevelWon.AddListener(LevelWinHandle);
        SDK_EventSystem.OnLevelLost.AddListener(LevelLoseHandle);
    }

    private void OnDisable()
    {
        SDK_EventSystem.OnLevelStarted.RemoveListener(LevelStartHandle);
        SDK_EventSystem.OnLevelWon.RemoveListener(LevelWinHandle);
        SDK_EventSystem.OnLevelLost.RemoveListener(LevelLoseHandle);
    }

    public void LeaveLevelHandle()
    {
        if (_isLeaving) return;
        _isLeaving = true;


        OutsideEventManager.KeyValueSetup eventSetup = new OutsideEventManager.KeyValueSetup()
        {
            keyValues = new OutsideEventManager.KeyValue[] {
                new OutsideEventManager.KeyValue()
                {
                    key = "level_index",
                    value = LevelManager.Instance.Database.LevelIndex,
                },
                new OutsideEventManager.KeyValue()
                {
                    key = "level_number",
                    value = LevelManager.Instance.Database.LevelNumber,
                },
                new OutsideEventManager.KeyValue()
                {
                    key = "progress",
                    value = EnemyManager.Instance.GetDeadEnemiesRatio(),
                },
                new OutsideEventManager.KeyValue()
                {
                    key = "time",
                    value = GameplayTimer.Instance.Timer,
                }
            }
        };

        OutsideEventManager.SendNameParamsEvent("level_leave", eventSetup, _isLogEnabled);
    }

    private void LevelStartHandle(int levelNumber, int levelIndex)
    {
        OutsideEventManager.KeyValueSetup eventSetup = new OutsideEventManager.KeyValueSetup()
        {
            keyValues = new OutsideEventManager.KeyValue[] {
                new OutsideEventManager.KeyValue()
                {
                    key = "level_index",
                    value = levelIndex,
                },
                new OutsideEventManager.KeyValue()
                {
                    key = "level_number",
                    value = levelNumber,
                }
            }
        };

        OutsideEventManager.SendNameParamsEvent("level_start", eventSetup, _isLogEnabled);
    }

    private void LevelLoseHandle(int levelNumber, int levelIndex, float progress, float time)
    {
        OutsideEventManager.KeyValueSetup eventSetup = new OutsideEventManager.KeyValueSetup()
        {
            keyValues = new OutsideEventManager.KeyValue[] {
                new OutsideEventManager.KeyValue()
                {
                    key = "level_index",
                    value = levelIndex,
                },
                new OutsideEventManager.KeyValue()
                {
                    key = "level_number",
                    value = levelNumber,
                },
                new OutsideEventManager.KeyValue()
                {
                    key = "progress",
                    value = progress,
                },
                new OutsideEventManager.KeyValue()
                {
                    key = "time",
                    value = time,
                }
            }
        };

        OutsideEventManager.SendNameParamsEvent("level_lose", eventSetup, _isLogEnabled);
    }

    private void LevelWinHandle(int levelNumber, int levelIndex, float time)
    {
        OutsideEventManager.KeyValueSetup eventSetup = new OutsideEventManager.KeyValueSetup()
        {
            keyValues = new OutsideEventManager.KeyValue[] {
                new OutsideEventManager.KeyValue()
                {
                    key = "level_index",
                    value = levelIndex - 1,
                },
                new OutsideEventManager.KeyValue()
                {
                    key = "level_number",
                    value = levelNumber - 1,
                },
                new OutsideEventManager.KeyValue()
                {
                    key = "time",
                    value = time,
                }
            }
        };

        OutsideEventManager.SendNameParamsEvent("level_win", eventSetup, _isLogEnabled);
    }
}
