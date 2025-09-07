using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Meta.Weapons;

public class OnWeaponTypeEquippedTransition : StateTransition, IActorIniter
{
    [SerializeField] private WeaponClass _weaponType;
    private EquippedWeaponDef _equippedWeapon;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetProperty(out _equippedWeapon)) {}
    }

    public override void Enter()
    {
        base.Enter();
        CheckEquippedWeapon();
    }

    public void CheckEquippedWeapon()
    {
        if (_equippedWeapon.Value.Class == _weaponType)
            DoTransition();
    }
}
