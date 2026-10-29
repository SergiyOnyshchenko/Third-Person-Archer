using System;
using System.Collections.Generic;
using Meta.Economy;
using Meta.Weapons;
using UI.Core;
using UnityEngine;

/// <summary>A card on the Guild Board: a completed campaign level to replay, with an order class.</summary>
public sealed class ContractCard
{
    public int Slot { get; }
    public MissionData Mission { get; }
    public ZoneData Zone { get; }
    public int ZoneIndex { get; }
    public WeaponClass OrderClass { get; }

    public ContractCard(int slot, MissionData mission, ZoneData zone, int zoneIndex, WeaponClass orderClass)
    {
        Slot = slot;
        Mission = mission;
        Zone = zone;
        ZoneIndex = zoneIndex;
        OrderClass = orderClass;
    }
}

public readonly struct GuildRankReward
{
    public readonly int Rank;
    public readonly GuildRankRewardType Type;
    public readonly int Amount;    // gold or essence
    public readonly WeaponDef Skin; // Skin only

    public GuildRankReward(int rank, GuildRankRewardType type, int amount, WeaponDef skin)
    {
        Rank = rank;
        Type = type;
        Amount = amount;
        Skin = skin;
    }
}

public interface IContractBoardService
{
    int SlotCount { get; }
    ContractCard GetCard(int slot);

    /// <summary>Fills empty slots with new cards (played cards are replaced on return to the board).</summary>
    void EnsureFilled();

    bool IsFreeRefreshAvailable { get; }
    /// <summary>Replaces all cards. Free once a day; afterwards call it after a rewarded ad with free = false.</summary>
    void RefreshAll(bool free);

    int DailyBonusLeft { get; }

    bool IsClassPlayable(WeaponClass cls);
    int GetPlayerDamage(WeaponClass cls);

    /// <summary>Fixed at launch: order bonus, daily bonus, reward multiplier and player damage.</summary>
    ContractRunInfo BuildRunInfo(ContractCard card, WeaponClass playedClass);
    BalanceConfig.MissionRewardResult GetRewardPreview(ContractCard card, WeaponClass playedClass);
    int GetReputationPreview(ContractCard card, WeaponClass playedClass);

    int Reputation { get; }
    int Rank { get; }
    int MaxRank { get; }
    int RepIntoRank { get; }
    int RepForNextRank { get; }

    bool TryGetPendingRankReward(out GuildRankReward reward);
    /// <summary>Grants the oldest unclaimed rank reward; chestClass is used for an essence chest.</summary>
    bool ClaimPendingRankReward(WeaponClass chestClass);
}

public sealed class ContractBoardService : IContractBoardService
{
    private readonly GuildData _data;
    private readonly BalanceConfig _balance;
    private readonly MissionProgressData _progress;
    private readonly ILoadoutWeaponStatService _weaponStats;

    public ContractBoardService(
        GuildData data,
        BalanceConfig balance,
        MissionProgressData progress,
        ILoadoutWeaponStatService weaponStats)
    {
        _data = data;
        _balance = balance;
        _progress = progress;
        _weaponStats = weaponStats;

        _data.EnsureSlotCount(Guild.SlotCount);
    }

    private BalanceConfig.GuildModule Guild => _balance.Guild;

    // ── Board ────────────────────────────────────────────────────────────────

    public int SlotCount => _data.Slots.Count;

    public ContractCard GetCard(int slot)
    {
        if (slot < 0 || slot >= _data.Slots.Count) return null;

        var s = _data.Slots[slot];
        if (s.IsEmpty || !TryFindCampaignMission(s.MissionId, out var mission, out var zone, out int zoneIndex))
            return null;

        return new ContractCard(slot, mission, zone, zoneIndex, (WeaponClass)s.OrderClass);
    }

    public void EnsureFilled()
    {
        var candidates = GetCandidates();
        if (candidates.Count == 0) return;

        for (int i = 0; i < _data.Slots.Count; i++)
        {
            if (GetCard(i) != null) continue;

            var pick = PickMission(candidates);
            _data.SetSlot(i, pick.mission.ID, (int)PickOrderClass());
        }
    }

    public bool IsFreeRefreshAvailable => _data.IsFreeRefreshAvailable();

    public void RefreshAll(bool free)
    {
        _data.ClearAllSlots(useFreeRefresh: free);
        EnsureFilled();
    }

    public int DailyBonusLeft => Mathf.Max(0, Guild.DailyBonusContracts - _data.GetDailyBonusUsed());

    // ── Weapons & rewards ────────────────────────────────────────────────────

    public bool IsClassPlayable(WeaponClass cls)
    {
        if (ServiceLocator.TryResolve<IWeaponClassUnlockService>(out var unlocks) && !unlocks.IsClassUnlocked(cls))
            return false;

        return GetPlayerDamage(cls) > 0;
    }

    public int GetPlayerDamage(WeaponClass cls) =>
        _weaponStats != null ? Mathf.RoundToInt(_weaponStats.GetEquippedDamage(cls)) : 0;

    public ContractRunInfo BuildRunInfo(ContractCard card, WeaponClass playedClass)
    {
        bool orderMatched = card != null && card.OrderClass == playedClass;
        bool dailyBonus = DailyBonusLeft > 0;

        return new ContractRunInfo(
            card != null ? card.Slot : -1,
            orderMatched,
            dailyBonus,
            Guild.GetRewardMultiplier(orderMatched, dailyBonus),
            GetPlayerDamage(playedClass));
    }

    public BalanceConfig.MissionRewardResult GetRewardPreview(ContractCard card, WeaponClass playedClass)
    {
        var run = BuildRunInfo(card, playedClass);
        return _balance.GetContractReward(card != null ? card.ZoneIndex : 0, playedClass, run.RewardMultiplier);
    }

    public int GetReputationPreview(ContractCard card, WeaponClass playedClass) =>
        Guild.GetReputation(card != null && card.OrderClass == playedClass);

    // ── Rank ─────────────────────────────────────────────────────────────────

    public int Reputation => _data.Reputation;
    public int Rank => Guild.GetRank(_data.Reputation);
    public int MaxRank => Guild.MaxRank;
    public int RepIntoRank => _data.Reputation - Guild.GetRepForRank(Rank);
    public int RepForNextRank => Rank >= Guild.MaxRank ? 0 : Guild.GetRepForNextRank(Rank);

    public bool TryGetPendingRankReward(out GuildRankReward reward)
    {
        int rank = _data.ClaimedRank + 1;
        if (rank > Rank)
        {
            reward = default;
            return false;
        }

        var skin = FindNextLockedSkin();
        var type = Guild.GetRankRewardType(rank, skin != null);
        reward = new GuildRankReward(rank, type, Guild.GetRankRewardAmount(rank, type), type == GuildRankRewardType.Skin ? skin : null);
        return true;
    }

    public bool ClaimPendingRankReward(WeaponClass chestClass)
    {
        if (!TryGetPendingRankReward(out var reward)) return false;

        switch (reward.Type)
        {
            case GuildRankRewardType.Gold:
                Economy.Wallet?.Add(CurrencyType.Cash, reward.Amount);
                break;

            case GuildRankRewardType.EssenceChest:
                Economy.Wallet?.Add(WeaponCurrencyUtility.GetTokenCurrency(chestClass), reward.Amount);
                break;

            case GuildRankRewardType.Skin:
                var progression = WeaponsInitializer.Instance != null ? WeaponsInitializer.Instance.Progression : null;
                if (progression == null || !progression.GrantSkin(reward.Skin))
                    return false;
                break;
        }

        _data.SetClaimedRank(reward.Rank);
        return true;
    }

    /// <summary>Skins in catalog order, classes the player can already use first.</summary>
    private WeaponDef FindNextLockedSkin()
    {
        var catalog = _balance.WeaponCatalog;
        var progression = WeaponsInitializer.Instance != null ? WeaponsInitializer.Instance.Progression : null;
        if (catalog == null || progression == null) return null;

        WeaponDef fallback = null;
        foreach (var def in catalog.All)
        {
            if (def == null || !def.IsSkin || progression.IsOwned(def.Id)) continue;
            if (IsClassPlayable(def.Class)) return def;
            fallback ??= def;
        }
        return fallback;
    }

    // ── Generation ───────────────────────────────────────────────────────────

    private List<(MissionData mission, int zoneIndex)> GetCandidates()
    {
        var result = new List<(MissionData, int)>();
        var zones = _progress.AllZones;

        for (int z = 0; z < zones.Count; z++)
        {
            var campaign = zones[z] != null ? zones[z].GetSegmentByType(MissionType.Campaign) : null;
            if (campaign == null) continue;

            foreach (var m in campaign.Missions)
                if (m != null && !string.IsNullOrEmpty(m.ID) && campaign.IsMissionCompletedOnce(m))
                    result.Add((m, z));
        }

        return result;
    }

    /// <summary>RecentZonesChance from the two latest zones with completed levels, the rest from older ones; no duplicates on the board when possible.</summary>
    private (MissionData mission, int zoneIndex) PickMission(List<(MissionData mission, int zoneIndex)> candidates)
    {
        var onBoard = new HashSet<string>();
        foreach (var s in _data.Slots)
            if (!s.IsEmpty) onBoard.Add(s.MissionId);

        var free = candidates.FindAll(c => !onBoard.Contains(c.mission.ID));
        if (free.Count == 0) free = candidates;

        int maxZone = 0;
        foreach (var c in free) maxZone = Mathf.Max(maxZone, c.zoneIndex);

        var recent = free.FindAll(c => c.zoneIndex >= maxZone - 1);
        var older = free.FindAll(c => c.zoneIndex < maxZone - 1);

        var pool = older.Count == 0 || UnityEngine.Random.value < Guild.RecentZonesChance ? recent : older;
        return pool[UnityEngine.Random.Range(0, pool.Count)];
    }

    /// <summary>Weighted toward classes that are below R of their next campaign mission in the current zone.</summary>
    private WeaponClass PickOrderClass()
    {
        var classes = new List<WeaponClass>();
        var weights = new List<float>();
        float total = 0f;

        foreach (WeaponClass cls in Enum.GetValues(typeof(WeaponClass)))
        {
            if (!IsClassPlayable(cls)) continue;

            float w = IsLaggingInCampaign(cls) ? Guild.LaggingClassOrderWeight : 1f;
            classes.Add(cls);
            weights.Add(w);
            total += w;
        }

        if (classes.Count == 0) return WeaponClass.Bow;

        float roll = UnityEngine.Random.value * total;
        for (int i = 0; i < classes.Count; i++)
        {
            roll -= weights[i];
            if (roll <= 0f) return classes[i];
        }
        return classes[classes.Count - 1];
    }

    private bool IsLaggingInCampaign(WeaponClass cls)
    {
        int zoneIndex = _progress.IsHeroicUnlocked() ? _progress.ZoneIndex : _progress.GetLastUnlockedZoneIndex();
        var zones = _progress.AllZones;
        if (zoneIndex < 0 || zoneIndex >= zones.Count || zones[zoneIndex] == null) return false;

        var zone = zones[zoneIndex];
        var campaign = zone.GetSegmentByType(MissionType.Campaign);
        if (campaign == null) return false;

        foreach (var m in campaign.Missions)
        {
            if (m == null || _progress.IsCampaignLevelCleared(zone, m)) continue;

            var missionClass = m.UseFixedWeaponClass ? m.FixedWeaponClass : m.BaseWeaponClass;
            if (missionClass != cls) continue;

            var ctx = new MissionContext(zone, zoneIndex, MissionType.Campaign, m, -1, -1, cls);
            return GetPlayerDamage(cls) < _balance.GetCampaignRecommendedDamage(ctx);
        }

        return false;
    }

    private bool TryFindCampaignMission(string id, out MissionData mission, out ZoneData zone, out int zoneIndex)
    {
        var zones = _progress.AllZones;
        for (int z = 0; z < zones.Count; z++)
        {
            var campaign = zones[z] != null ? zones[z].GetSegmentByType(MissionType.Campaign) : null;
            if (campaign == null) continue;

            foreach (var m in campaign.Missions)
            {
                if (m == null || m.ID != id) continue;

                mission = m;
                zone = zones[z];
                zoneIndex = z;
                return true;
            }
        }

        mission = null;
        zone = null;
        zoneIndex = 0;
        return false;
    }
}
