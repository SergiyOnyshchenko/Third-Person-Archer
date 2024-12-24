using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private ActorController[] _targets;

    public static EnemyManager Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this);
    }

    private void Start()
    {
        InitEnemies();
    }

    private void InitEnemies()
    {
        List<ActorController> targets = new List<ActorController>();

        Enemy[] enemies = FindObjectsOfType<Enemy>(true);
        
        foreach (var enemy in enemies)
        {
            ActorController target = enemy.GetComponent<ActorController>();

            if (target != null)
                targets.Add(target);
        }

        _targets = targets.ToArray();
    }

    public float GetDeadEnemiesRatio()
    {
        int deadEnemies = 0;

        foreach (var target in _targets)
        {
            if (target != null && target.IsDead)
                deadEnemies++;
        }

        return (float) deadEnemies / (float) _targets.Length;
    }
}
