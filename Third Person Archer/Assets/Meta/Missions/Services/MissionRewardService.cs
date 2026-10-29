using System;
using Meta.Weapons;
using UnityEngine;

public sealed class MissionRewardService : IMissionRewardService
{
    private readonly BalanceConfig _balance;
    private readonly MetaLoopProgressData _loopData;

    public MissionRewardService(BalanceConfig balance, MetaLoopProgressData loopData)
    {
        _balance = balance;
        _loopData = loopData;
    }

    public MissionReward Calculate(MissionContext ctx)
    {
        if (ctx == null || !ctx.IsValid)
            return new MissionReward(0, null);

        var mission = ctx.Mission;
        bool hasOverride = mission != null && mission.UseRewardOverride;

        int rewardIndex = GetRewardIndex(ctx);
        WeaponClass missionWeaponClass = ctx.RequiredWeaponClass;

        if (ctx.SelectedType == MissionType.Contracts)
        {
            // Guild Board: by the level's zone, essence of the played class, order/daily bonus fixed at launch.
            float multiplier = ctx.Contract != null ? ctx.Contract.RewardMultiplier : 1f;
            var contract = _balance.GetContractReward(ctx.ZoneIndex, missionWeaponClass, multiplier);
            return new MissionReward(contract.Cash, NormalizeTokens(contract.TokensPerType));
        }

        if (!hasOverride)
        {
            // Unified reward from BalanceConfig (no per-type helper methods, no randomness).
            var result = _balance.GetMissionReward(
                ctx.SelectedType,
                rewardIndex,
                missionWeaponClass);

            // If tokens array is all zeros, you may pass null to match older UI expectations.
            // I keep the previous behavior: campaign used null tokens.
            int totalCash = result.Cash;
            int[] raw = result.TokensPerType;

            if (IsBossReplay(ctx))
            {
                totalCash = _balance.Rounding.RoundMoneyToNiceInt(totalCash * _balance.Heroic.BossReplayCashMultiplier);
                raw = ScaleTokens(raw, _balance.Heroic.BossReplayTokenMultiplier);
            }

            if (IsCampaignFinale(ctx))
            {
                var finale = _balance.GetMissionReward(MissionType.Boss, Mathf.Max(0, ctx.ZoneIndex), missionWeaponClass);
                totalCash += finale.Cash;
                raw = AddTokens(raw, finale.TokensPerType);
            }

            int[] tokens = NormalizeTokens(raw);
            return new MissionReward(totalCash, tokens);
        }

        // -----------------------------
        // Per-mission override behavior
        // -----------------------------
        // - Campaign: money + ONLY mission token type
        // - Sniper/Boss: money + ALL token types (uniform)

        int cash;
        int[] tokensOut;

        switch (ctx.SelectedType)
        {
            case MissionType.Campaign:
            {
                cash = _balance.Rounding.RoundMoneyToNiceInt(mission.OverrideBaseCash);

                // Campaign tokens (only mission weapon class)
                int classCount = Enum.GetValues(typeof(WeaponClass)).Length;
                tokensOut = new int[classCount];

                float baseTokens = mission.OverrideBaseTokensPerType;
                int perType = Mathf.RoundToInt(_balance.Rounding.RoundTokensToNice(baseTokens));

                int wi = (int)missionWeaponClass;
                if (wi >= 0 && wi < tokensOut.Length)
                    tokensOut[wi] = Mathf.Max(0, perType);

                tokensOut = NormalizeTokens(tokensOut);
                return new MissionReward(cash, tokensOut);
            }

            case MissionType.Sniper:
            {
                cash = _balance.Rounding.RoundMoneyToNiceInt(mission.OverrideBaseCash);
                tokensOut = BuildUniformTokens(mission.OverrideBaseTokensPerType);
                tokensOut = NormalizeTokens(tokensOut);
                return new MissionReward(cash, tokensOut);
            }

            case MissionType.Boss:
            {
                cash = _balance.Rounding.RoundMoneyToNiceInt(mission.OverrideBaseCash);
                tokensOut = BuildUniformTokens(mission.OverrideBaseTokensPerType);
                tokensOut = NormalizeTokens(tokensOut);
                return new MissionReward(cash, tokensOut);
            }

            default:
                return new MissionReward(0, null);
        }
    }

    private int GetRewardIndex(MissionContext ctx)
    {
        // Reward index = the progression index for that mode
        // Campaign: global campaign index
        // Contracts: handled in Calculate (zone of the replayed level)
        // Sniper: sniper completed index
        // Boss: zone index (reward grows with zone; its tokens are the "key" to the next zone's tier)
        switch (ctx.SelectedType)
        {
            case MissionType.Campaign:
                // Heroic continues the campaign curve.
                return Mathf.Max(0, ctx.GlobalCampaignIndex) + (ctx.IsHeroic ? _balance.Heroic.RewardIndexOffset : 0);

            case MissionType.Sniper:
            {
                // Fixed per mission (its place among all Sniper missions), so replays (Heroic: every zone
                // is open) don't grow the reward forever. Counter only as a fallback.
                int global = MissionProgressData.Active != null
                    ? MissionProgressData.Active.GetGlobalSegmentIndex(MissionType.Sniper, ctx.Mission)
                    : -1;
                return global >= 0 ? global : Mathf.Max(0, _loopData != null ? _loopData.SniperCompletedIndex : 0);
            }

            case MissionType.Boss:
                return Mathf.Max(0, ctx.ZoneIndex);

            default:
                return 0;
        }
    }

    /// <summary>
    /// First clear of the campaign's last level (last level of the boss-less final zone): it also pays
    /// that zone's Boss reward. Checked before progress is applied, so the preview and the payout match.
    /// </summary>
    private bool IsCampaignFinale(MissionContext ctx)
    {
        if (ctx.SelectedType != MissionType.Campaign || ctx.IsHeroic || !_balance.Heroic.FinaleBossReward)
            return false;

        var zone = ctx.Zone;
        var campaign = zone?.GetSegmentByType(MissionType.Campaign);
        if (zone.HasBoss() || campaign?.Missions == null || campaign.Missions.Count == 0)
            return false;

        var last = campaign.Missions[campaign.Missions.Count - 1];
        return ctx.Mission == last && !campaign.IsMissionCompletedOnce(last);
    }

    /// <summary>Boss already beaten (replay from a browsed zone): reduced reward, the tier "key" was paid once.</summary>
    private static bool IsBossReplay(MissionContext ctx) =>
        ctx.SelectedType == MissionType.Boss && ctx.Zone != null && ctx.Zone.IsBossCompleted();

    private static int[] ScaleTokens(int[] tokens, float multiplier)
    {
        if (tokens == null) return null;
        var scaled = new int[tokens.Length];
        for (int i = 0; i < tokens.Length; i++)
            scaled[i] = Mathf.RoundToInt(tokens[i] * multiplier);
        return scaled;
    }

    private static int[] AddTokens(int[] a, int[] b)
    {
        if (a == null) return b;
        if (b == null) return a;

        var sum = new int[Mathf.Max(a.Length, b.Length)];
        for (int i = 0; i < sum.Length; i++)
            sum[i] = (i < a.Length ? a[i] : 0) + (i < b.Length ? b[i] : 0);
        return sum;
    }

    private int[] BuildUniformTokens(float rawPerType)
    {
        int classCount = Enum.GetValues(typeof(WeaponClass)).Length;
        var tokens = new int[classCount];

        int perType = Mathf.RoundToInt(_balance.Rounding.RoundTokensToNice(rawPerType));
        perType = Mathf.Max(0, perType);

        for (int i = 0; i < classCount; i++)
            tokens[i] = perType;

        return tokens;
    }

    private static int[] NormalizeTokens(int[] tokens)
    {
        if (tokens == null || tokens.Length == 0)
            return null;

        bool any = false;
        for (int i = 0; i < tokens.Length; i++)
        {
            if (tokens[i] > 0)
            {
                any = true;
                break;
            }
        }

        return any ? tokens : null;
    }
}