using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Meta.Weapons;

namespace Actor.Properties
{
    public class BoomerangAmmo : Ammo<BoomerangController>
    {
        public override WeaponClass WeaponType => WeaponClass.Boomerang;
    }
}