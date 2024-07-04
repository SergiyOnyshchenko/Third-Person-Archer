using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor; 

public class EnemyOffScreenUIManager : MonoBehaviour
{
    [SerializeField] private EnemyOffScreenUI _prefab;
    private List<EnemyOffScreenUI> _enemies = new List<EnemyOffScreenUI>();
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    private void OnEnable()
    {
        EnemyEventSystem.OnEnemyActivated.AddListener(AddEnemy);
    }

    private void OnDisable()
    {
        EnemyEventSystem.OnEnemyActivated.RemoveListener(AddEnemy);
    }

    private void Update()
    {
        foreach (var enemy in _enemies)
        {
            
        }
    }

    public void AddEnemy(ITarget target)
    {
        EnemyOffScreenUI enemy = Instantiate(_prefab, transform);
        enemy.Init(target);
        _enemies.Add(enemy);    
    }
}
