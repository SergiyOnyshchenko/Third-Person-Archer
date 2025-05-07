using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponUpgradeViewManager : MonoBehaviour
{
    [SerializeField] private WeaponInventoryData _inventoryData;
    [Space]
    [SerializeField] private GameObject _bowUpgrade;
    [SerializeField] private GameObject _crossbowUpgrade;
    [SerializeField] private GameObject _spearUpgrade;

    private void OnEnable()
    {
        UpdateView();
    }

    private void UpdateView()
    {
        _bowUpgrade.SetActive(_inventoryData.HasWeapon(WeaponType.Bow));
        _crossbowUpgrade.SetActive(_inventoryData.HasWeapon(WeaponType.Crossbow));
        _spearUpgrade.SetActive(_inventoryData.HasWeapon(WeaponType.Spear));
    }
}
