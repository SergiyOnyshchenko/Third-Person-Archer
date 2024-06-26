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
        WeaponData[] weapons = _database.GetWeaponsByID(_inventory.Load());

        if (weapons == null || weapons.Length == 0)
            return;

        _inventory.Init(weapons);
    }
}
