using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuWeaponSelector : MonoBehaviour, IWeaponSelector, ISelectable, ISelector, IEquipable, IEquipper, ILockable
{
    [field: SerializeField] public WeaponData WeaponData { get; private set; }
    [Header("View")]
    [SerializeField] private TextMeshProUGUI _nameField;
    [SerializeField] private Image _icon;
    private Button _button;
    public ISelector Selector => this;
    public IEquipper Equipper => this;
    public bool IsSelected { get; private set; }
    public event Action<IWeaponSelector> OnWeaponSelected;
    public event Action OnSelected;
    public event Action OnDeselected;
    public event Action OnEquipped;
    public event Action OnUnequipped;
    public event Action OnLocked;
    public event Action OnUnlocked;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(TrySelect);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(TrySelect);
    }

    private void Start()
    {
        Init(WeaponData);
    }

    public void Init(WeaponData weaponData)
    {
        _nameField.text = weaponData.Name;
        _icon.sprite = weaponData.SkinData.Icon;

        if (weaponData.State == WeaponState.Locked)
        {
            OnLocked?.Invoke();
        }
        else
        {
            OnUnlocked?.Invoke();
        }
    }

    public void Select()
    {
        
        IsSelected = true;
        OnSelected?.Invoke();
    }

    public void Deselect()
    {
        IsSelected = false;
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

    private void TrySelect()
    {
        if (WeaponData.State == WeaponState.Locked)
            return;

        OnWeaponSelected?.Invoke(this);
    }
}
