using System;
using UnityEngine;

namespace Meta.Weapons
{
    [Serializable]
    public class AcquisitionRule
    {
        public bool IsVIPOnly;
        public bool CanBuyWithCash = true;
        public bool CanBuyWithGold = true;
        public int CashPrice;
        public int GoldPrice;
    }

    [CreateAssetMenu(menuName = "Meta/Weapons/Weapon Def", fileName = "WeaponDef")]
    public class WeaponDef : ScriptableObject
    {
        [SerializeField] private string _id = "weapon_id";
        [SerializeField] private string _displayName = "Weapon Name";
        [SerializeField] private WeaponClass _class;
        [SerializeField] private WeaponStats _baseStats;
        [SerializeField] private AcquisitionRule _acquisition = new();
        [SerializeField] private int _unlockAfterCampaignLevel = 0; // Bow unlocked at start -> 0

        [Tooltip("Parts usable by THIS weapon (not global), ordered into slots if you like.")]
        [SerializeField] private WeaponPartDef[] _parts;

        public string Id => _id;
        public string DisplayName => _displayName;
        public WeaponClass Class => _class;
        public WeaponStats BaseStats => _baseStats;
        public AcquisitionRule Acquisition => _acquisition;
        public int UnlockAfterCampaignLevel => _unlockAfterCampaignLevel;
        public WeaponPartDef[] Parts => _parts ?? Array.Empty<WeaponPartDef>();
    }
}

