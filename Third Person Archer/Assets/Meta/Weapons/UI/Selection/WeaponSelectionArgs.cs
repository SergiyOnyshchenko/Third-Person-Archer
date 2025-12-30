using System;

namespace Meta.Weapons.UI
{
    [Serializable]
    public sealed class WeaponSelectionArgs
    {
        public WeaponClass WeaponClass { get; }
        public int CampaignLevel { get; }
        public string PreselectWeaponId { get; }

        public WeaponSelectionArgs(WeaponClass weaponClass, int campaignLevel, string preselectWeaponId = null)
        {
            WeaponClass = weaponClass;
            CampaignLevel = campaignLevel;
            PreselectWeaponId = preselectWeaponId;
        }
    }
}