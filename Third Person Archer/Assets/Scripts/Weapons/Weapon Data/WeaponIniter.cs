using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponIniter : MonoBehaviour
{
    [SerializeField] private WeaponDatabase _database;
    [SerializeField] private WeaponInventoryData _inventory;

    private void Awake()
    {
        Init();
    }

    public void Init()
    {
        _database.Load();
        _inventory.Init(_database.GetEquippedWeapons());
    }
}
