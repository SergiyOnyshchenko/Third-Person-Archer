using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class LockEventsReciever : MonoBehaviour
{
    private ILockable _lockable;
    public UnityEvent OnLocked = new UnityEvent();
    public UnityEvent OnUnlocked = new UnityEvent();

    private void Awake()
    {
        _lockable = GetComponent<ILockable>();
    }

    private void OnEnable()
    {
        _lockable.OnLocked += SendLockEvent;
        _lockable.OnUnlocked += SendUnlockEvent;
    }

    private void OnDisable()
    {
        _lockable.OnLocked -= SendLockEvent;
        _lockable.OnUnlocked -= SendUnlockEvent;
    }

    private void SendLockEvent()
    {
        OnLocked?.Invoke();
    }

    private void SendUnlockEvent()
    {
        OnUnlocked?.Invoke();
    }
}
