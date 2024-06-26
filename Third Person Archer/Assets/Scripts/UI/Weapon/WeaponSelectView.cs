using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponSelectView : MonoBehaviour, ISelectable, ISelector, IEquipable, IEquipper
{
    [SerializeField] private WeaponData _weaponData;
    [Header("View")]
    [SerializeField] private TextMeshProUGUI _nameField;
    private Button _button;
    public bool IsSelected { get; private set; }
    public WeaponData Data { get => _weaponData; }

    public event Action OnSelected;
    public event Action OnDeselected;
    public event Action OnEquipped;
    public event Action OnUnequipped;

    private void Awake()
    {
        _button = GetComponent<Button>();
        Init(_weaponData);
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    public void Init(WeaponData weaponData)
    {
        _nameField.text = weaponData.Name;
    }

    public void Select()
    {
        OnSelected?.Invoke();
    }

    public void Deselect()
    {
        OnDeselected?.Invoke();
    }

    public void Equip()
    {
        OnEquipped?.Invoke();
    }

    public void Unquip()
    {
        OnUnequipped?.Invoke();
    }
}
