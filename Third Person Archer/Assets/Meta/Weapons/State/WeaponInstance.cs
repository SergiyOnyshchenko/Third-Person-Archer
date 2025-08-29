// Assets/Scripts/Meta/Weapons/State/WeaponInstance.cs
using System;
using System.Collections.Generic;

namespace Meta.Weapons
{
    [Serializable]
    public class WeaponInstance
    {
        public string WeaponId;
        public bool Owned;
        public bool Equipped;

        // partId -> current level (0 means not upgraded; next is 1..Max)
        public Dictionary<string, int> PartLevels = new();

        // partId -> mastery tier achieved (0 = none; then 1..Mastery.MaxTier)
        public Dictionary<string, int> MasteryTiers = new();
    }

    [Serializable]
    public class WeaponsState
    {
        public List<WeaponInstance> Weapons = new();
        public Dictionary<string, bool> ClassEquippedMap = new(); // optional visibility; actual equip is per weapon in Weapons list
    }
}

