using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Meta.Weapons;

namespace Actor.Properties
{
    public class SpearAmmo : Ammo<SpearController>
    {
        public override WeaponClass WeaponType => WeaponClass.Spear;
    }
}
