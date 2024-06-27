using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actor.Properties
{
    public class SpearAmmo : Ammo<SpearController>
    {
        public override WeaponType WeaponType => WeaponType.Spear;
    }
}
