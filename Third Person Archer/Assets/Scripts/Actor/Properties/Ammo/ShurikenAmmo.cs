using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using Meta.Weapons;

namespace Actor.Properties
{
    public class ShurikenAmmo : Ammo<ShurikenController>
    {
        public override WeaponClass WeaponType => WeaponClass.Shuriken;
    }
}