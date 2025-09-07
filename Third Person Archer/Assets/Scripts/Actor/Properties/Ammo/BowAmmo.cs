using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;
using Meta.Weapons;

namespace Actor.Properties
{
    public class BowAmmo : Ammo<BowController>
    {
        public override WeaponClass WeaponType => WeaponClass.Bow;
    }
}
