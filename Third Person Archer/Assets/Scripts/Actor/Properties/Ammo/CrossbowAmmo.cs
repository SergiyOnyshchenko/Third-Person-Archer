using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class CrossbowAmmo : Ammo<CrossbowController>
    {
        public override WeaponType WeaponType => WeaponType.Crossbow;
    }
}
