using System;

namespace Meta.Weapons
{
    [Serializable]
    public sealed class WeaponClassUnlockedPopupArgs
    {
        public string Title;
        public WeaponClass WeaponClass;
    }
}