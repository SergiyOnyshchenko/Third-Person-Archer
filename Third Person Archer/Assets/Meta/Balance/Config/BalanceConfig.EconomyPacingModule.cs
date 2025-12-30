using System;
using UnityEngine;

public partial class BalanceConfig
{
    [Serializable]
    public sealed class EconomyPacingModule
    {
        [Serializable]
        public sealed class ZoneOverride
        {
            [Tooltip("Zone index this override applies to (0-based).")]
            public int ZoneIndex;

            [Tooltip("Override settings for this zone.")]
            public Settings Values = new Settings();
        }

        [Serializable]
        public sealed class Settings
        {
            [Header("Pacing Targets (Editor Generator)")]
            [Tooltip("Target upgrades per campaign mission (you want 1.0). Used by the price generator tool.")]
            [Min(0.1f)] public float TargetUpgradesPerCampaignMission = 1f;

            [Tooltip("How many campaign missions after a weapon unlock should the player typically afford purchasing it (choice vs upgrades). You chose ~2.")]
            [Min(1f)] public float PurchaseAffordWindowMissions = 2f;

            [Tooltip("How much of upgrade 'pressure' comes from money vs tokens. 0.5 = balanced.")]
            [Range(0f, 1f)] public float UpgradeMoneyPressure = 0.5f;

            [Tooltip("Budget share of forecast income allocated to upgrades. Remaining budget goes to purchases.")]
            [Range(0f, 1f)] public float UpgradeBudgetShare = 0.65f;

            [Header("Expected Side Modes (Forecast)")]
            [Tooltip("Expected number of contract missions per campaign mission (0 = ignore).")]
            [Min(0f)] public float ExpectedContractsPerCampaign = 0f;

            [Tooltip("Expected number of sniper missions per campaign mission (0 = ignore).")]
            [Min(0f)] public float ExpectedSnipersPerCampaign = 0f;
        }

        [Header("Default Settings")]
        [SerializeField] private Settings _default = new Settings();

        [Header("Per-Zone Overrides (Optional)")]
        [Tooltip("If an entry matches the current zone index, the generator will use those settings instead of default.")]
        [SerializeField] private ZoneOverride[] _zoneOverrides;

        public Settings Default => _default;

        public Settings GetZoneSettings(int zoneIndex)
        {
            if (_zoneOverrides == null)
                return _default;

            for (int i = 0; i < _zoneOverrides.Length; i++)
            {
                var z = _zoneOverrides[i];
                if (z != null && z.ZoneIndex == zoneIndex && z.Values != null)
                    return z.Values;
            }

            return _default;
        }
    }
}