using System;
using Meta.Weapons;
using UnityEngine;

public sealed class MissionRewardService : IMissionRewardService
{
    private readonly BalanceConfig _balance;
    private readonly ILoopProgress _loop;
    private readonly MetaLoopProgressData _loopData;

    public MissionRewardService(BalanceConfig balance, ILoopProgress loop, MetaLoopProgressData loopData)
    {
        _balance = balance;
        _loop = loop;
        _loopData = loopData;
    }

    public MissionReward Calculate(MissionContext ctx)
    {
        if (ctx == null || !ctx.IsValid)
            return new MissionReward(0, null);

        // Prefer ctx.BalanceLoopIndex (already computed by your meta runtime),
        // but keep _loop as a fallback.
        int balanceLoop = ctx.BalanceLoopIndex;
        if (balanceLoop < 0 && _loop != null)
            balanceLoop = _loop.GetBalanceLoopIndex();

        var mission = ctx.Mission;
        bool hasOverride = mission != null && mission.UseRewardOverride;

        int rewardIndex = GetRewardIndex(ctx);
        WeaponClass missionWeaponClass = ctx.RequiredWeaponClass;

        if (!hasOverride)
        {
            // Unified reward from BalanceConfig (no per-type helper methods, no randomness).
            var result = _balance.GetMissionReward(
                ctx.SelectedType,
                rewardIndex,
                missionWeaponClass,
                balanceLoop);

            // If tokens array is all zeros, you may pass null to match older UI expectations.
            // I keep the previous behavior: campaign used null tokens.
            int[] tokens = NormalizeTokens(result.TokensPerType);
            return new MissionReward(result.Cash, tokens);
        }

        // -----------------------------
        // Per-mission override behavior
        // -----------------------------
        // Requirement from you:
        // - Override values scale with each loop.
        // - Campaign: money + ONLY mission token type
        // - Contracts/Sniper/Boss: money + ALL token types (uniform)

        var lm = _balance.GetLoopMultipliers(balanceLoop);

        int cash;
        int[] tokensOut;

        switch (ctx.SelectedType)
        {
            case MissionType.Campaign:
            {
                cash = _balance.Rounding.RoundMoneyToNiceInt(mission.OverrideBaseCash * lm.CampaignCash);

                // Campaign tokens (only mission weapon class)
                int classCount = Enum.GetValues(typeof(WeaponClass)).Length;
                tokensOut = new int[classCount];

                float baseTokens = mission.OverrideBaseTokensPerType * lm.CampaignTokens;
                int perType = Mathf.RoundToInt(_balance.Rounding.RoundTokensToNice(baseTokens));

                int wi = (int)missionWeaponClass;
                if (wi >= 0 && wi < tokensOut.Length)
                    tokensOut[wi] = Mathf.Max(0, perType);

                tokensOut = NormalizeTokens(tokensOut);
                return new MissionReward(cash, tokensOut);
            }

            case MissionType.Contracts:
            {
                cash = _balance.Rounding.RoundMoneyToNiceInt(mission.OverrideBaseCash * lm.ContractCash);
                tokensOut = BuildUniformTokens(mission.OverrideBaseTokensPerType * lm.ContractTokens);
                tokensOut = NormalizeTokens(tokensOut);
                return new MissionReward(cash, tokensOut);
            }

            case MissionType.Sniper:
            {
                cash = _balance.Rounding.RoundMoneyToNiceInt(mission.OverrideBaseCash * lm.SniperCash);
                tokensOut = BuildUniformTokens(mission.OverrideBaseTokensPerType * lm.SniperTokens);
                tokensOut = NormalizeTokens(tokensOut);
                return new MissionReward(cash, tokensOut);
            }

            case MissionType.Boss:
            {
                cash = _balance.Rounding.RoundMoneyToNiceInt(mission.OverrideBaseCash * lm.BossCash);
                tokensOut = BuildUniformTokens(mission.OverrideBaseTokensPerType * lm.BossTokens);
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
        // Contracts: contracts completed index
        // Sniper: sniper completed index
        // Boss: use 0 (lump sum configured as constant progression)
        switch (ctx.SelectedType)
        {
            case MissionType.Campaign:
                return Mathf.Max(0, ctx.GlobalCampaignIndex);

            case MissionType.Contracts:
                return Mathf.Max(0, _loopData != null ? _loopData.ContractsCompletedIndex : 0);

            case MissionType.Sniper:
                return Mathf.Max(0, _loopData != null ? _loopData.SniperCompletedIndex : 0);

            case MissionType.Boss:
                return 0;

            default:
                return 0;
        }
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