using System;

namespace Meta.Weapons.UI
{
    public enum WeaponHighlightMode { None, Upgrade, Buy }

    [Serializable]
    public sealed class WeaponSelectionArgs
    {
        public WeaponClass WeaponClass { get; }
        public int CampaignLevel { get; }
        public string PreselectWeaponId { get; }
        public WeaponHighlightMode HighlightMode { get; }

        public WeaponSelectionArgs(WeaponClass weaponClass, int campaignLevel,
            string preselectWeaponId = null,
            WeaponHighlightMode highlightMode = WeaponHighlightMode.None)
        {
            WeaponClass = weaponClass;
            CampaignLevel = campaignLevel;
            PreselectWeaponId = preselectWeaponId;
            HighlightMode = highlightMode;
        }
    }
}