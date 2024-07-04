using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Actor;

public static class EnemyEventSystem 
{
    public static UnityEvent<ITarget> OnEnemyActivated = new UnityEvent<ITarget>();
    public static void SendEnemyActivated(ITarget target) => OnEnemyActivated?.Invoke(target);
}
