// UpgradePriceProfile.cs
using System;
using UnityEngine;
using Meta.Economy;

namespace Meta.Weapons
{
    //[CreateAssetMenu(menuName = "Meta/Weapons/Upgrade Price Profile", fileName = "UpgradePriceProfile")]
    [System.Serializable]
    public class UpgradePriceProfile 
    {
        [Header("Purchase (one-time)")]
        [Tooltip("Multiplier applied to WeaponDef.PurchaseCash when computing final cash cost.")]
        [Min(0)]
        [SerializeField] private float _purchaseCashMultiplier = 1f;

        [Tooltip("Multiplier applied to WeaponDef.PurchaseTokens when computing final token cost.")]
        [Min(0)]
        [SerializeField] private float _purchaseTokenMultiplier = 1f;

        [Header("Upgrade cost range")]
        [Tooltip("Cash cost at level 0 -> 1.")]
        [Min(0)]
        [SerializeField] private int _startUpgradeCash = 10;

        [Tooltip("Cash cost at max-1 -> max.")]
        [Min(0)]
        [SerializeField] private int _maxUpgradeCash = 1000;

        [Tooltip("Token cost at level 0 -> 1.")]
        [Min(0)]
        [SerializeField] private int _startUpgradeTokens = 1;

        [Tooltip("Token cost at max-1 -> max.")]
        [Min(0)]
        [SerializeField] private int _maxUpgradeTokens = 50;

        [Header("Curve 0..1 for upgrade price")]
        [Tooltip("X = normalized upgrade level (0..1), Y = price factor (0..1)")]
        [SerializeField] private AnimationCurve _priceCurve01 =
            AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Header("Ad Upgrade")]
        [Tooltip("If true, this weapon system allows upgrading a level by watching an ad.")]
        [SerializeField] private bool _canUpgradeWithAd = true;

        public float PurchaseCashMultiplier => _purchaseCashMultiplier;
        public float PurchaseTokenMultiplier => _purchaseTokenMultiplier;

        public int StartUpgradeCash => _startUpgradeCash;
        public int MaxUpgradeCash   => _maxUpgradeCash;
        public int StartUpgradeTokens => _startUpgradeTokens;
        public int MaxUpgradeTokens   => _maxUpgradeTokens;

        public AnimationCurve PriceCurve01 => _priceCurve01;
        public bool CanUpgradeWithAd => _canUpgradeWithAd;

        public void EvaluateUpgradeCost(int currentLevel, int maxLevel,
                                        out int cash, out int tokens)
        {
            if (maxLevel <= 0)
            {
                cash = tokens = 0;
                return;
            }

            int clamped = Mathf.Clamp(currentLevel, 0, maxLevel - 1);
            float tNorm = (float)clamped / (maxLevel - 1);
            float curve = _priceCurve01 != null ? _priceCurve01.Evaluate(tNorm) : tNorm;
            curve = Mathf.Clamp01(curve);

            cash   = Mathf.RoundToInt(Mathf.Lerp(_startUpgradeCash,   _maxUpgradeCash,   curve));
            tokens = Mathf.RoundToInt(Mathf.Lerp(_startUpgradeTokens, _maxUpgradeTokens, curve));
        }
    }
}