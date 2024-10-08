using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;
using UnityEngine.Events;

public class UnlockWeaponState : MainState, IActorIniter
{
    [Space]
    [SerializeField] private WeaponData _weaponData;
    private ActorController _actor;

    public UnityEvent<WeaponData> OnUnlocked = new UnityEvent<WeaponData>();
    public UnityEvent OnAlreadyUnlocked = new UnityEvent();

    public override void Enter()
    {
        base.Enter();
        TryUnlockWeapon();
    }

    private void TryUnlockWeapon()
    {
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
    }

    public void InitActor(ActorController actor)
    {
        _actor = actor;
    }
}
