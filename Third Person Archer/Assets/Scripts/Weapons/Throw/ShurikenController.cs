using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Actor.Properties;

public class ShurikenController : ThrowWeaponController
{
    protected override void InitWeaponHolders(ActorController actor)
    {
        if (actor.TryGetProperty(out ShurikenAmmo ammo))
            _ammoCount = ammo;

        if (actor.TryGetPropertys(out ShurikenHolder[] holders))
            foreach (var holder in holders)
                if (holder.Pov == PovType.FirstPerson)
                    _weaponHolder = holder;
    }
}
