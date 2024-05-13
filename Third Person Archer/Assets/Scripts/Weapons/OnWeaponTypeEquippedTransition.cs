using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class OnWeaponTypeEquippedTransition : StateTransition, IActorIniter
{
    [SerializeField] private WeaponType _weaponType;
    private WeaponInventory _inventory;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out WeaponInventory inventory))
            _inventory = inventory;
    }

    public override void Enter()
    {
        base.Enter();
        CheckEquippedWeapon();
    }

    public void CheckEquippedWeapon()
    {
        if (_inventory.EquippedWeaponType == _weaponType)
            DoTransition();
    }
}
