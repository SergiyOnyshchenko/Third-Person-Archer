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

        // Upgrade level belongs to the class (0..cap of the best owned tier), not to a model.
        public List<WeaponClassLevel> ClassLevels = new();

        // Rewarded ads watched towards each skin.
        public List<SkinAdProgress> SkinAds = new();

        // 0 = per-weapon levels (old saves), 1 = per-class levels.
        public int ProgressionVersion;
    }

    [Serializable]
    public class WeaponClassLevel
    {
        public WeaponClass Class;
        public int Level;
    }

    [Serializable]
    public class SkinAdProgress
    {
        public string WeaponId;
        public int AdsWatched;
    }

    [Serializable]
    public class WeaponInstance
    {
        public string WeaponId;
        public bool Owned;

        /// <summary>
        /// Old per-weapon upgrade level. Read only when migrating saves (ProgressionVersion 0);
        /// the live level is WeaponsState.ClassLevels.
        /// </summary>
        public int UpgradeLevel;
    }
}