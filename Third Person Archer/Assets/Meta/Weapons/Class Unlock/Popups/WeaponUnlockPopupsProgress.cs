using System;
using System.Collections.Generic;

namespace Meta.Weapons
{
    [Serializable]
    public sealed class WeaponUnlockPopupsProgress
    {
        public List<string> shownWeaponIds = new();
        public List<int> shownWeaponClasses = new(); // WeaponClass as int
    }
}