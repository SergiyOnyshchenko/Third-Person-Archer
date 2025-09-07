using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;
using TMPro;
using Meta.Weapons;

public class AmmoCountView<W> : MonoBehaviour, IActorIniter, IAmmoCountView where W : WeaponController
{
    [SerializeField] private WeaponClass _weaponType;
    [SerializeField] private TextMeshProUGUI _countField;
    private Ammo<W> _ammo;
    public WeaponClass WeaponType => _weaponType;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out Ammo<W> ammo))
        {
            _ammo = ammo;
            _ammo.OnAmmoModified.AddListener(ModifyView);
            ModifyView();
        }
    }

    private void ModifyView()
    {
        _countField.text = _ammo.AmmoCount.ToString(); 
    }
}

