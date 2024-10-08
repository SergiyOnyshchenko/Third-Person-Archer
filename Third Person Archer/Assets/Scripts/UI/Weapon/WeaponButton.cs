using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.UI;

public class WeaponButton : MonoBehaviour, IActorIniter
{
    [SerializeField] private WeaponType _weaponType;
    private Button _button;
    private IWeaponEquipper _weaponEquipper;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void OnEnable()
    {
        _button.onClick.AddListener(TryEquipWeapon);
    }

    private void OnDisable()
    {
        _button.onClick.RemoveListener(TryEquipWeapon);
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
        }
    }

    private void TryEquipWeapon()
    {
        _weaponEquipper.TryEquip(_weaponType);
    }
}
