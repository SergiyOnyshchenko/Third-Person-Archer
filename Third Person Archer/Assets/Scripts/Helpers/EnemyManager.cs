using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private ITarget[] _targets;

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
        List<ITarget> targets = new List<ITarget>();

        Enemy[] enemies = FindObjectsOfType<Enemy>(true);

        foreach (var enemy in enemies)
        {
            Actor.Target target = enemy.GetComponentInChildren<Actor.Target>();

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

        return deadEnemies / _targets.Length;
    }
}
