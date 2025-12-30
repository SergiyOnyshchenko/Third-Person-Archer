using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;
using UnityEngine.UI;

public class CrosshairView : MonoBehaviour, IActorIniter
{
    [SerializeField] private Transform _parent;
    [SerializeField] private Transform _scalePart;
    [Space]
    [SerializeField] private float _minScale = 1;
    [SerializeField] private float _maxScale = 1.5f;
    private WeaponPull _weaponPull;
    private EquippedWeaponDef _equippedWeapon;
    private ShootTypeOverride _shootTypeOverride;
    private SpringFloat _springFloat;

    private void Start()
    {
        _springFloat = new SpringFloat(25f, 0.3f, _minScale);
    }

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out _weaponPull)) { }
        if (actor.TryGetProperty(out _shootTypeOverride)) { }

        if (actor.TryGetProperty(out _equippedWeapon))
        {
            CheckShootTypeView();
            _equippedWeapon.OnPropertyChanged += CheckShootTypeView;
        }
    }

    private void OnDestroy()
    {
        if (_equippedWeapon != null)
            _equippedWeapon.OnPropertyChanged -= CheckShootTypeView;
    }

    private void FixedUpdate()
    {
        float scale = Mathf.Lerp(_minScale, _maxScale, _weaponPull.Value);
        _springFloat.UpdateValue(scale);
        _scalePart.localScale = new Vector3(_springFloat.Value, _springFloat.Value, _springFloat.Value);
    }

    private void CheckShootTypeView()
    {
        if (_shootTypeOverride != null)
        {
            if (_shootTypeOverride.Value == ShootType.Direct)
                ShowView(true);
            else
                ShowView(false);
        }
        else
        {
            if (_equippedWeapon != null && _equippedWeapon.Value != null)
            {
                if (_equippedWeapon.Value.Class == Meta.Weapons.WeaponClass.Crossbow || 
                    _equippedWeapon.Value.Class == Meta.Weapons.WeaponClass.Shuriken)
                    ShowView(true);
                else
                    ShowView(false);
            }
            else
            {
                ShowView(false);
            }
        }
    }

    private void ShowView(bool value)
    {
        _parent.gameObject.SetActive(value);
    }
}
