using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StartLevelCallback : MonoBehaviour
{
    public UnityEvent OnLevelStarted = new UnityEvent(); 

    private void OnEnable()
    {
        LevelEventSystem.OnLevelStarted.AddListener(Invoke);
    }

    private void OnDisable()
    {
        LevelEventSystem.OnLevelStarted.RemoveListener(Invoke);
    }

    public void Invoke()
    {
        OnLevelStarted.Invoke();
    }
}
