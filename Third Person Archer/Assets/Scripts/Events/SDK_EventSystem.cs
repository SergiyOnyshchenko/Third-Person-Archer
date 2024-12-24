using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.Events;

public static class SDK_EventSystem 
{
    public static UnityEvent<int, int> OnLevelStarted = new UnityEvent<int, int>();

    public static UnityEvent<int, int, float> OnLevelWon = new UnityEvent<int, int, float>();

    public static UnityEvent<int, int, float, float> OnLevelLost = new UnityEvent<int, int, float, float>();

    public static void SendLevelStarted(int level_number, int level_index)
    {
        OnLevelStarted?.Invoke(level_number, level_index);
    }

    public static void SendLevelWin(int level_number, int level_index, float time) => OnLevelWon?.Invoke(level_number, level_index, time);
    public static void SendLevelLose(int level_number, int level_index, float progress, float time) => OnLevelLost?.Invoke(level_number, level_index, progress, time);
}
