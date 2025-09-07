using System;
using System.Collections.Generic;
using UnityEngine;
using Meta.Weapons;

namespace Meta.Weapons.Unlocks
{
    [Serializable]
    public sealed class WeaponClassUnlockState
    {
        [Serializable]
        public sealed class Entry
        {
            public WeaponClass Class;
            public bool Unlocked;
            public List<string> CreditedMissions = new(); 
        }

        public List<Entry> Entries = new();

        public Entry GetOrCreate(WeaponClass cls)
        {
            var e = Entries.Find(x => x.Class == cls);
            if (e == null)
            {
                e = new Entry { Class = cls, Unlocked = false, CreditedMissions = new List<string>() };
                Entries.Add(e);
            }
            return e;
        }
    }
}
