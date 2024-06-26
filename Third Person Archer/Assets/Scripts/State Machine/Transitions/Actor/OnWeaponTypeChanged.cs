using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class OnWeaponTypeChanged : StateTransition, IActorIniter
{
    private WeaponInventory _inventory;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out WeaponInventory inventory))
            _inventory = inventory;
    }

    public override void Enter()
    {
        base.Enter();
        _inventory.OnWeaponChanged.AddListener(DoTransition);
    }

    public override void Exit()
    {
        _inventory.OnWeaponChanged.RemoveListener(DoTransition);
        base.Exit();
    }
}
