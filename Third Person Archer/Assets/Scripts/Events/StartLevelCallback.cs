using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StartLevelCallback : MonoBehaviour
{
    public UnityEvent OnLevelStarted = new UnityEvent(); 

    private void OnEnable()
    {
        RuntimeMissionEventManager.OnGameStarted.AddListener(Invoke);
    }

    private void OnDisable()
    {
        RuntimeMissionEventManager.OnGameStarted.RemoveListener(Invoke);
    }

    public void Invoke()
    {
        OnLevelStarted.Invoke();
    }
}
