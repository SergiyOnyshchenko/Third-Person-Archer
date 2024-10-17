using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.Properties;

public class AmmoEmptyTransition<T> : StateTransition, IActorIniter  where T : WeaponController
{
    protected Ammo<T> _ammoCount;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out Ammo<T> ammo))
        {
            _ammoCount = ammo;
        }
    }

    public override void Enter()
    {
        base.Enter();

        _ammoCount.OnAmmoModified.AddListener(AmmoCheck);
        AmmoCheck();
    }

    public override void Exit()
    {
        _ammoCount.OnAmmoModified.RemoveListener(AmmoCheck);

        base.Exit();
    }

    private void AmmoCheck()
    {
        if(_ammoCount.Value <= 0)
            DoTransition();
    }
}

