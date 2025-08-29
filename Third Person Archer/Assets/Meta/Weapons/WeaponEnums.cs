using System;

namespace Meta.Weapons
{
    public enum WeaponClass
    {
        Bow = 0,
        Crossbow = 1,
        Spear = 2,
        // Keep extensible. If your doc later adds a 4th class, just add here.
    }

    public enum WeaponStat
    {
        Damage,
        Balance,      // (a.k.a. stability/handling)
        Distance,     // effective range
        AmmoCount,
        ReloadTime,   // lower is better
        Zoom
    }

    public enum GateStatus
    {
        Ok,
        Warn,  // playable but below recommended
        Block  // required class not met
    }
}

