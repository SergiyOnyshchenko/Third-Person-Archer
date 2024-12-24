using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuWeaponSelectManager : MonoBehaviour
{
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private WeaponInventoryData _inventory;
    [SerializeField] private Button _equipButton;
    private IWeaponSelector[] _selectors;
    private IWeaponSelector _selectedWeapon;
    private IWeaponSelector _equippedWeapon;

    private void Awake()
    {
        _selectors = GetComponentsInChildren<IWeaponSelector>();
    }

    private void OnEnable()
    {
        foreach (var selector in _selectors)
            selector.OnWeaponSelected += Select;

        _equipButton.onClick.AddListener(EquipCurrentSelected);

        StartCoroutine(Init());
    }

    private void OnDisable()
    {
        foreach (var selector in _selectors)
            selector.OnWeaponSelected -= Select;

        _equipButton.onClick.RemoveListener(EquipCurrentSelected);

        if (_equippedWeapon != null)
        {
            Equip(_equippedWeapon);
            Select(_equippedWeapon);
        }
    }

    public IEnumerator Init()
    {
        yield return null;

        WeaponData equippedWeapon = _inventory.GetWeaponByType(_weaponType);

        if (equippedWeapon != null)
        {
            foreach (var selector in _selectors)
                if (selector.WeaponData == equippedWeapon)
                {
                    Equip(selector);
                    Select(selector);
                }
        }
    }

    private void Select(IWeaponSelector target)
    {
        foreach (var selector in _selectors)
        {
            if (selector.WeaponData == target.WeaponData)
            {
                selector.Selector.Select();
                _selectedWeapon = selector;

                _inventory.EquipWeapon(_selectedWeapon.WeaponData);
            }
            else
                selector.Selector.Deselect();
        }

        UpdateEquipButton();
    }

    private void Equip(IWeaponSelector target)
    {
        foreach (var selector in _selectors)
        {
            if (selector.WeaponData == target.WeaponData)
            {
                selector.Equipper.Equip();
                _equippedWeapon = selector;
                _inventory.AddNewWeapon(_equippedWeapon.WeaponData);
            }         
            else
                selector.Equipper.Unquip();
        }

        UpdateEquipButton();
    }

    private void EquipCurrentSelected()
    {
        Equip(_selectedWeapon);

        if (AppMetricaEventReporter.Instance != null)
            AppMetricaEventReporter.Instance.SendWeaponEquip(_selectedWeapon.WeaponData);
    }

    private void UpdateEquipButton()
    {
        if (_selectedWeapon != _equippedWeapon)
            _equipButton.gameObject.SetActive(true);
        else
            _equipButton.gameObject.SetActive(false);
    }
}
