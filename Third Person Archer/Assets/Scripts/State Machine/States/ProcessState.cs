using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ProcessState : MainState
{
    public UnityEvent OnProcessFinished;

    protected void FinishProcess()
    {
        OnProcessFinished?.Invoke();
    }
}
