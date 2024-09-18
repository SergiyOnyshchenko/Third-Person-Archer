using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Actor;

public static class HostageEventSystem 
{
    public static UnityEvent OnHostageDied = new UnityEvent();
    public static void SendHostageDied() => OnHostageDied?.Invoke();
}
