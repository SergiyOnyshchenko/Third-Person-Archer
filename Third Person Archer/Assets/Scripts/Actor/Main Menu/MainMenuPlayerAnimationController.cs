using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using UnityEngine;
using UnityEngine.Events;
using UI.Core;
using Meta.Weapons;

public class MainMenuPlayerAnimationController : MonoBehaviour, IActorIniter
{
    [SerializeField] private UINavigator _mainMenu;
    [SerializeField] private string _weaponsScreen = "weapons";
    private Actor.Animator _animator;
    private EquippedWeaponDef _equippedWeaponDef;
    public UnityEvent OnWeaponEquipped = new UnityEvent();
    public UnityEvent OnWeaponUnequipped = new UnityEvent();
    [Space]
    public UnityEvent OnBowEquipped = new UnityEvent();
    public UnityEvent OnCrossbowEquipped = new UnityEvent();
    public UnityEvent OnSpearEquipped = new UnityEvent();
    public UnityEvent OnShurkenEquipped = new UnityEvent();
    public UnityEvent OnBoomerangEquipped = new UnityEvent();

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _animator)) { }
        if (actor.TryGetProperty(out _equippedWeaponDef)) { }
    }

    private void OnEnable()
    {
        _mainMenu.ScreenOpened += MainMenuScreenOpenHandler;
        _mainMenu.ScreenClosed += MainMenuScreenClosedHandler;
    }

    private void OnDisable()
    {
        _mainMenu.ScreenOpened -= MainMenuScreenOpenHandler;
        _mainMenu.ScreenClosed -= MainMenuScreenClosedHandler;
    }

    private void MainMenuScreenOpenHandler(ScreenView screen)
    {
        if (screen.ScreenId == _weaponsScreen)
        {
            EquipWeapon();  
            _equippedWeaponDef.OnPropertyChanged += EquipWeapon;    
        }
    }

    private void MainMenuScreenClosedHandler(ScreenView screen)
    {
        if (screen.ScreenId == _weaponsScreen)
        {
            UnequipWeapon();
            _equippedWeaponDef.OnPropertyChanged -= EquipWeapon;
        }
    }

    private void EquipWeapon()
    {
        var weaponClass = _equippedWeaponDef.Value.Class;

        _animator.SetInteger("WeaponClass", (int)weaponClass);
        _animator.SetTrigger("ShowWeapon");

        switch (weaponClass)
        {
            case WeaponClass.Bow:
                OnBowEquipped?.Invoke();
                break;
            case WeaponClass.Crossbow:
                OnCrossbowEquipped?.Invoke();
                break;
            case WeaponClass.Spear:
                OnSpearEquipped?.Invoke();
                break;
            case WeaponClass.Shuriken:
                OnShurkenEquipped?.Invoke();
                break;
            case WeaponClass.Boomerang:
                OnBoomerangEquipped?.Invoke();
                break;
        }

        OnWeaponEquipped?.Invoke();
    }

    private void UnequipWeapon()
    {
        _animator.SetTrigger("HideWeapon");
        OnWeaponUnequipped?.Invoke();
    }
}