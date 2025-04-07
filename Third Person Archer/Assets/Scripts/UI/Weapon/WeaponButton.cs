using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.UI;

public class WeaponButton : MonoBehaviour, IActorIniter
{
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private KeyCode _keyboardKey;
    [Header("View")]
    [SerializeField] private GameObject _selectedView;
    [SerializeField] private GameObject _unSelectedView;
    private Button _button;
    private WeaponInventory _weaponEquipper;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(TryEquipWeapon);

        if(_weaponEquipper != null)
            _weaponEquipper.OnWeaponChanged.AddListener(UpdateSelectionView);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(TryEquipWeapon);

        if (_weaponEquipper != null)
            _weaponEquipper.OnWeaponChanged.RemoveListener(UpdateSelectionView);
    }

    private void Update()
    {
        if (UnityEngine.Input.GetKeyDown(_keyboardKey))
            TryEquipWeapon();
    }

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out WeaponInventory inventory))
        {
            _weaponEquipper = inventory;

            WeaponData data = inventory.WeaponsData.GetWeaponByType(_weaponType);
            if(data == null) 
            { 
                gameObject.SetActive(false);
            }

            inventory.OnWeaponChanged.AddListener(UpdateSelectionView);
            UpdateSelectionView();
        }
    }

    private void TryEquipWeapon()
    {
        _weaponEquipper.TryEquip(_weaponType);
    }

    private void UpdateSelectionView()
    {
        SelectView(_weaponEquipper.EquippedWeaponType == _weaponType);
    }

    private void SelectView(bool select)
    {
        _selectedView.SetActive(select);
        _unSelectedView.SetActive(!select);
    }
}
