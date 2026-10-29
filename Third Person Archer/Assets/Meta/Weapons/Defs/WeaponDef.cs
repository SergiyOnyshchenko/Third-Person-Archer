using System;
using UnityEngine;

namespace Meta.Weapons
{
    [CreateAssetMenu(menuName = "Meta/Weapons/Weapon Def", fileName = "WeaponDef")]
    public class WeaponDef : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string _id = "weapon_id";
        [SerializeField] private string _displayName = "Weapon Name";
        [SerializeField] private Sprite _icon;
        [SerializeField] private WeaponClass _class;

        [Header("Progression")]
        [Tooltip("Power tier 1..4 within the class. 0 = skin: cosmetic only, power comes from the best owned tier.")]
        [Min(0)]
        [SerializeField] private int _tier = 1;

        [Tooltip("Skins only: rewarded ads to watch to unlock this skin.")]
        [Min(1)]
        [SerializeField] private int _skinAdsRequired = 3;

        [Header("Unlock")]
        [Tooltip("Campaign level index (global) after which this weapon becomes visible/purchasable. 0 = available from start.")]
        [SerializeField] private int _unlockAfterCampaignLevel = 0;

        [Space]
        [Space]
        [Space]

        [Header("Base & Max Stats")]
        [Tooltip("Stats at upgrade level 0.")]
        [SerializeField] private WeaponStats _baseStats;

        [Tooltip("Stats at max upgrade level.")]
        [SerializeField] private WeaponStats _maxStats;

        [Tooltip("How many discrete upgrade steps (0..MaxUpgradeLevel). 0 = not upgradeable.")]
        [Min(0)]
        [SerializeField] private int _maxUpgradeLevel = 10;

        [Tooltip("Curve from 0..1 representing progression between BaseStats -> MaxStats.\n" +
                 "X = normalized level (0..1), Y = curve factor (0..1).")]
        [SerializeField] private AnimationCurve _upgradeCurve01 =
            AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Space]
        [Space]
        [Space]

        [Header("Purchase Cost (per weapon)")]
        [Tooltip("One-time cash cost to buy this weapon.")]
        [Min(0)]
        [SerializeField] private int _purchaseCash = 0;

        [Tooltip("One-time class token cost to buy this weapon.")]
        [Min(0)]
        [SerializeField] private int _purchaseTokens = 0;

        [Header("Upgrade Price Profile")]
        [Tooltip("Per-weapon upgrade price profile. Different weapons can share or have unique profiles.")]
        [SerializeField] private UpgradePriceProfile _upgradePriceProfile;

        // --- Public API ---

        public string Id => _id;
        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public WeaponClass Class => _class;

        public int Tier => _tier;
        public bool IsSkin => _tier <= 0;
        public int SkinAdsRequired => _skinAdsRequired;

        public int UnlockAfterCampaignLevel => _unlockAfterCampaignLevel;

        public WeaponStats BaseStats => _baseStats;
        public WeaponStats MaxStats => _maxStats;
        public int MaxUpgradeLevel => _maxUpgradeLevel;
        public AnimationCurve UpgradeCurve01 => _upgradeCurve01;

        public int PurchaseCash => _purchaseCash;
        public int PurchaseTokens => _purchaseTokens;

        public UpgradePriceProfile UpgradePriceProfile => _upgradePriceProfile;

        /// <summary>
        /// Returns normalized level in [0,1] for this weapon.
        /// </summary>
        public float GetNormalizedLevel(int upgradeLevel)
        {
            if (_maxUpgradeLevel <= 0) return 0f;
            upgradeLevel = Mathf.Clamp(upgradeLevel, 0, _maxUpgradeLevel);
            return (float)upgradeLevel / _maxUpgradeLevel;
        }

        /// <summary>
        /// Damage at a normalized position inside this weapon's level range (same curve as StatsService).
        /// </summary>
        public float GetDamageAt01(float t01)
        {
            float t = Mathf.Clamp01(t01);
            float curve = _upgradeCurve01 != null ? Mathf.Clamp01(_upgradeCurve01.Evaluate(t)) : t;
            return Mathf.Lerp(_baseStats.Damage, _maxStats.Damage, curve);
        }
    }
}