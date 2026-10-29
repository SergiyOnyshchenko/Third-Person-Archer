using System;
using UnityEngine;

public partial class BalanceConfig
{
    /// <summary>
    /// Guild Board (Contracts): "rest and farm" mode. Replayed campaign levels with the player's own
    /// weapon, no upward auto-scaling, reward by the level's zone, guild reputation and ranks.
    /// </summary>
    [Serializable]
    public sealed class GuildModule
    {
        [Header("Board")]
        [Min(1)] public int SlotCount = 3;

        [Tooltip("Chance that a new card comes from the two latest unlocked zones (the rest: older zones).")]
        [Range(0f, 1f)] public float RecentZonesChance = 0.7f;

        [Tooltip("Order class weight for a class that is below R of its next campaign mission (others weigh 1).")]
        [Min(1f)] public float LaggingClassOrderWeight = 3f;

        [Header("Reward multipliers")]
        [Tooltip("Reward multiplier when the contract is played with the order class.")]
        [Min(1f)] public float OrderClassRewardMultiplier = 1.5f;

        [Tooltip("Reward multiplier of the first contracts of the day.")]
        [Min(1f)] public float DailyBonusMultiplier = 2f;

        [Min(0)] public int DailyBonusContracts = 5;

        [Header("Enemies")]
        [Tooltip("Enemy damage multiplier vs the campaign formula (the formula itself is disabled for now).")]
        [Min(0f)] public float EnemyDamageMultiplier = 0.6f;

        [Tooltip("Enemies never need more than this many body shots of the played class.")]
        [Min(1)] public int MaxBodyShots = 2;

        [Header("Reputation")]
        [Min(0)] public int ReputationPerContract = 10;
        [Min(0)] public int OrderClassReputationBonus = 5;

        [Header("Ranks (rank r → r+1 costs RankBaseRep + RankRepStep × (r − 1))")]
        [Min(1)] public int MaxRank = 20;
        [Min(1)] public int RankBaseRep = 100;
        [Min(0)] public int RankRepStep = 50;

        [Header("Rank rewards (rank r ≥ 2)")]
        [Tooltip("Every N-th rank gives a skin (while locked skins remain). Other ranks alternate essence chest / gold.")]
        [Min(1)] public int SkinEveryRanks = 3;
        [Min(0)] public int GoldBase = 150;
        [Min(0)] public int GoldPerRank = 50;
        [Min(0)] public int ChestBase = 8;
        [Min(0)] public int ChestPerRank = 2;

        public int GetReputation(bool orderMatched) =>
            ReputationPerContract + (orderMatched ? OrderClassReputationBonus : 0);

        public float GetRewardMultiplier(bool orderMatched, bool dailyBonus) =>
            (orderMatched ? OrderClassRewardMultiplier : 1f) * (dailyBonus ? DailyBonusMultiplier : 1f);

        /// <summary>Reputation needed to go from rank to rank + 1.</summary>
        public int GetRepForNextRank(int rank) =>
            RankBaseRep + RankRepStep * Mathf.Max(0, rank - 1);

        /// <summary>Total reputation at which the rank is reached (rank 1 = 0).</summary>
        public int GetRepForRank(int rank)
        {
            int total = 0;
            for (int r = 1; r < Mathf.Min(rank, MaxRank); r++)
                total += GetRepForNextRank(r);
            return total;
        }

        public int GetRank(int reputation)
        {
            int rank = 1;
            int need = GetRepForNextRank(rank);
            while (rank < MaxRank && reputation >= need)
            {
                reputation -= need;
                rank++;
                need = GetRepForNextRank(rank);
            }
            return rank;
        }

        public GuildRankRewardType GetRankRewardType(int rank, bool skinAvailable)
        {
            if (rank % SkinEveryRanks == 0 && skinAvailable) return GuildRankRewardType.Skin;
            return rank % 2 == 0 ? GuildRankRewardType.EssenceChest : GuildRankRewardType.Gold;
        }

        public int GetRankRewardAmount(int rank, GuildRankRewardType type) => type switch
        {
            GuildRankRewardType.Gold => GoldBase + GoldPerRank * rank,
            GuildRankRewardType.EssenceChest => ChestBase + ChestPerRank * rank,
            _ => 0
        };
    }
}

public enum GuildRankRewardType
{
    Gold,
    EssenceChest, // essence of a class the player picks
    Skin,
}
