using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class LevelEventSystem
{
    public static UnityEvent<int> OnLoadLevel = new UnityEvent<int>();
    public static UnityEvent OnLoadNextLevel = new UnityEvent();
    public static UnityEvent OnReloadLevel = new UnityEvent();
    public static UnityEvent OnLoadMainMenu = new UnityEvent();
    public static UnityEvent OnLoadPreloader = new UnityEvent();

    public static void SendLoadLevel(int index) => OnLoadLevel?.Invoke(index);
    public static void SendLoadNextLevel() => OnLoadNextLevel?.Invoke();
    public static void SendReloadLevel() => OnReloadLevel?.Invoke();
    public static void SendLoadMainMenu() => OnLoadMainMenu?.Invoke();
    public static void SendLoadPreloader() => OnLoadPreloader?.Invoke();
}
