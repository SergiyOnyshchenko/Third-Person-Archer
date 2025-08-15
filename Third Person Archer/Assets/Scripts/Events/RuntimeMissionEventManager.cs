using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class RuntimeMissionEventManager
{
    public static UnityEvent OnGameStarted = new UnityEvent();
    public static UnityEvent OnGameContinued = new UnityEvent();
    public static UnityEvent OnGameFinished = new UnityEvent();

    public static void SendGameStarted()
    {
        OnGameStarted?.Invoke();
    }
    
    public static void SendGameContinued()
    {
        OnGameContinued?.Invoke();
    }

    public static void SendGameFinished()
    {
        OnGameFinished?.Invoke();
    }
}
