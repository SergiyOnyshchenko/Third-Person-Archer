using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class ShurikenThrowState : WeaponThrowState
{
    protected override void InitWeaponController(ActorController actor)
    {
        if (actor.TryGetSystem(out ShurikenController controller))
            _weaponController = controller;
    }
}
