using System;
using System.Collections.Generic;

namespace Meta.Weapons
{
    [Serializable]
    public class WeaponsState
    {
        public List<WeaponInstance> Weapons = new();

        // Per-class equipped weapon ids – must never be null/empty once loadout is initialized.
        public string EquippedBowId;
        public string EquippedCrossbowId;
        public string EquippedSpearId;
        public string EquippedShurikenId;
        public string EquippedBoomerangId;
    }

    [Serializable]
    public class WeaponInstance
    {
        public string WeaponId;
        public bool Owned;

        /// <summary>
        /// Current upgrade level, 0..WeaponDef.MaxUpgradeLevel.
        /// </summary>
        public int UpgradeLevel;
    }
}
