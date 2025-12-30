using System;
using UnityEngine;

public partial class BalanceConfig
{
    [Serializable]
    public sealed class LoopModule
    {
        [Header("Loop Settings")]
        [Tooltip("Highest loop index that has unique balance settings. Example: 2 means loops 0,1,2 have unique values. Loop 3+ reuse loop 2 settings.")]
        [SerializeField] private int _maxSupportedLoopIndex = 2;

        [Tooltip("List of difficulty/reward multipliers for each loop. Index = loop index.")]
        [SerializeField] private LoopMultipliers[] _loopMultipliers;

        public int ClampLoopIndex(int loopIndex)
        {
            if (_loopMultipliers == null || _loopMultipliers.Length == 0)
                return 0;

            int maxIndex = Mathf.Clamp(_maxSupportedLoopIndex, 0, _loopMultipliers.Length - 1);
            return Mathf.Clamp(loopIndex, 0, maxIndex);
        }

        public LoopMultipliers GetLoopMultipliers(int loopIndex)
        {
            if (_loopMultipliers == null || _loopMultipliers.Length == 0)
                return new LoopMultipliers();

            return _loopMultipliers[ClampLoopIndex(loopIndex)];
        }
    }

    [Serializable]
    public sealed class LoopMultipliers
    {
        [Header("Difficulty Multipliers")]
        [Tooltip("Multiplier applied to campaign difficulty for this loop.")]
        public float CampaignDifficulty = 1f;

        [Tooltip("Multiplier applied to contract difficulty for this loop.")]
        public float ContractDifficulty = 1f;

        [Tooltip("Multiplier applied to sniper difficulty for this loop.")]
        public float SniperDifficulty = 1f;

        [Header("Reward Multipliers")]
        [Tooltip("Multiplier applied to campaign cash rewards in this loop.")]
        public float CampaignCash = 1f;

        [Tooltip("Multiplier applied to campaign token rewards in this loop.")]
        public float CampaignTokens = 1f;

        [Tooltip("Multiplier applied to contract cash rewards in this loop.")]
        public float ContractCash = 1f;

        [Tooltip("Multiplier applied to sniper cash rewards in this loop.")]
        public float SniperCash = 1f;

        [Tooltip("Multiplier applied to boss cash rewards in this loop.")]
        public float BossCash = 1f;

        [Tooltip("Multiplier applied to contract token rewards in this loop.")]
        public float ContractTokens = 1f;

        [Tooltip("Multiplier applied to sniper token rewards in this loop.")]
        public float SniperTokens = 1f;

        [Tooltip("Multiplier applied to boss token rewards in this loop.")]
        public float BossTokens = 1f;

        [Header("Damage Gate Multiplier")]
        [Tooltip("Multiplier applied to required damage for campaign gate checks in this loop.")]
        public float GateDamage = 1f;

        [Header("Economy Grind (Editor tools)")]
        [Tooltip("Optional multiplier to make prices/grind higher in later loops. Used by editor generators.")]
        public float EconomyCost = 1f;
    }
}