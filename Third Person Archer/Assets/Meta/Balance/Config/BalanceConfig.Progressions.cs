using System;
using UnityEngine;

public partial class BalanceConfig
{
    public enum RewardChannel
    {
        Cash = 0,
        MissionToken = 1, // only token type matching mission weapon class
        AllTokens = 2     // same amount to all token types
    }

    /// <summary>
    /// Common progression: base + perStep * index, with optional deterministic spread.
    /// </summary>
    [Serializable]
    public sealed class LinearProgression
    {
        [Tooltip("Value at index = 0.")]
        public float Base = 0f;

        [Tooltip("Value added per step.")]
        public float PerStep = 0f;

        [Tooltip("Optional deterministic spread (±%). 0 = no randomness at all.")]
        [Range(0f, 1f)] public float RandomSpread = 0f;

        /// <param name="index">Step index (campaign global index, contracts completed, etc.)</param>
        /// <param name="determinismKey">Stable key used only if RandomSpread > 0</param>
        public float Evaluate(int index, int determinismKey)
        {
            float v = Base + PerStep * Mathf.Max(0, index);

            if (RandomSpread <= 0f)
                return v;

            // Deterministic "random" in [-Spread, +Spread]
            float t = Hash01(determinismKey);
            float r = Mathf.Lerp(-RandomSpread, RandomSpread, t);

            return v * (1f + r);
        }

        private static float Hash01(int x)
        {
            // Simple stable int hash -> 0..1
            unchecked
            {
                uint h = (uint)x;
                h ^= 2747636419u;
                h *= 2654435769u;
                h ^= h >> 16;
                h *= 2654435769u;
                h ^= h >> 16;
                h *= 2654435769u;
                return (h & 0x00FFFFFFu) / (float)0x01000000u;
            }
        }
    }

    [Serializable]
    public sealed class RewardProgression
    {
        [Tooltip("Which currency this progression affects.")]
        public RewardChannel Channel = RewardChannel.Cash;

        [Tooltip("Value curve for this currency.")]
        public LinearProgression Progression = new LinearProgression();
    }

    [Serializable]
    public sealed class MissionRewardProfile
    {
        [Tooltip("Mission type this profile applies to.")]
        public MissionType MissionType;

        [Tooltip("One or more currency progressions to apply. Example: Cash + MissionToken.")]
        public RewardProgression[] Entries;
    }

    public readonly struct MissionRewardResult
    {
        public readonly int Cash;
        public readonly int[] TokensPerType; // indexed by WeaponClass

        public MissionRewardResult(int cash, int[] tokensPerType)
        {
            Cash = cash;
            TokensPerType = tokensPerType;
        }
    }
}