using System.Collections;
using System.Collections.Generic;
using Actor.Properties;
using UnityEngine;

namespace Actor.Properties
{
    public class BowAmmo : Ammo<BowController>
    {
        public override WeaponType WeaponType => WeaponType.Bow;
    }
}
