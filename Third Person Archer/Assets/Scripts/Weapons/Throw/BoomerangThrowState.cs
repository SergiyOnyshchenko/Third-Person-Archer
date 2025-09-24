using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class BoomerangThrowState : WeaponThrowState
{
    protected override void InitWeaponController(ActorController actor)
    {
        if (actor.TryGetSystem(out BoomerangController controller))
            _weaponController = controller;
    }
}
