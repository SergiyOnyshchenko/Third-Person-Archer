using System.Collections;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

public class UnlockWeaponState : MainState, IActorIniter
{
    [Space]
    [SerializeField] private WeaponProgressionData _progressionData;
    [Header("View")]
    [SerializeField] private WeaponUnlockView _view;
    [Space]
    [SerializeField] private GameObject _nextLevelButton;
    [SerializeField] private GameObject _menuButton;
    private ActorController _actor;
    
    public override void Enter()
    {
        base.Enter();

        _progressionData.Load();

        DOVirtual.DelayedCall(1f, () =>
        {
            if (_progressionData.TryGetCurrentInstance(out WeaponProgressionInstance instance))
            {
                _view.Init(instance.WeaponData, _progressionData.CurrentProgress);
                _progressionData.IncreaseProgression(TryUnlockWeapon);
            }
            else
            {
                LevelEventSystem.SendLoadNextLevel();
            }
        });
    }

    private void TryUnlockWeapon(WeaponData weaponData, float ratio)
    {
        _view.Play(ratio);

        if (ratio >= 1)
        {
            if (_actor.TryGetSystem(out WeaponInventory inventory))
            {
                inventory.WeaponsData.AddNewWeapon(weaponData);
                inventory.WeaponsData.EquipWeapon(weaponData);
            }

            _menuButton.SetActive(true);
        }
        else
        {
            _nextLevelButton.SetActive(true);
        }

        /*
        if (_weaponData.State != WeaponState.Locked)
        {
            OnAlreadyUnlocked?.Invoke();
        }
        else
        {
            _weaponData.Unlock();

            if (_actor.TryGetSystem(out WeaponInventory inventory))
            {
                inventory.WeaponsData.AddNewWeapon(_weaponData);
                inventory.WeaponsData.EquipWeapon(_weaponData);
            }

            OnUnlocked?.Invoke(_weaponData);
        }
        */
    }

    public void InitActor(ActorController actor)
    {
        _actor = actor;
    }
}
