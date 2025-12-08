using System;

namespace Meta.Weapons.UI
{
    [Serializable]
    public sealed class WeaponSelectionArgs
    {
        public WeaponClass WeaponClass { get; }
        public int CampaignLevel { get; }

        public WeaponSelectionArgs(WeaponClass weaponClass, int campaignLevel)
        {
            WeaponClass = weaponClass;
            CampaignLevel = campaignLevel;
        }
    }
}