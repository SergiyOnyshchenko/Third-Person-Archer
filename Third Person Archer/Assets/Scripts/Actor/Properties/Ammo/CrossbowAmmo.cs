using System.Collections;
using System.Collections.Generic;
using Meta.Weapons;
using UnityEngine;

namespace Actor.Properties
{
    public class CrossbowAmmo : Ammo<CrossbowController>
    {
        public override WeaponClass WeaponType => WeaponClass.Crossbow;
    }
}
