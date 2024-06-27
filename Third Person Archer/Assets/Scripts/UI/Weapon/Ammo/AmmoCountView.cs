using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;
using TMPro;

public class AmmoCountView<W> : MonoBehaviour, IActorIniter where W : WeaponController
{
    [SerializeField] private WeaponType _weaponType;
    [SerializeField] private TextMeshProUGUI _countField;
    private Ammo<W> _ammo;

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
        _countField.text = _ammo.Value.ToString(); 
    }
}

