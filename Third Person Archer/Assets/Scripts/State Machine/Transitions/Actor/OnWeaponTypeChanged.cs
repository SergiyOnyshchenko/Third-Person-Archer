using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class OnWeaponTypeChanged : StateTransition, IActorIniter
{
    private EquippedWeaponDef _equippedWeapon;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out _equippedWeapon)){}
    }

    public override void Enter()
    {
        base.Enter();

        if (_equippedWeapon != null)
            _equippedWeapon.OnPropertyChanged += DoTransition;
    }

    public override void Exit()
    {
        if (_equippedWeapon != null)
            _equippedWeapon.OnPropertyChanged -= DoTransition;

        base.Exit();
    }
}
