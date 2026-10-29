using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Guild Board save: board cards, reputation, claimed rank rewards, daily bonus and free refresh.
/// Registered in DataManager (GameData.prefab) so it is reachable from both menu and gameplay scenes.
/// </summary>
[CreateAssetMenu(fileName = "GuildData", menuName = "GameMeta/GuildData")]
public sealed class GuildData : GameData
{
    private const string SaveKey = "guild_state";

    [Serializable]
    public sealed class ContractSlot
    {
        public string MissionId; // empty = card was played, refill on the board
        public int OrderClass;   // WeaponClass

        public bool IsEmpty => string.IsNullOrEmpty(MissionId);
    }

    [Serializable]
    public sealed class State
    {
        public List<ContractSlot> Slots = new();
        public int Reputation;
        public int ClaimedRank = 1; // rank rewards up to this rank are granted
        public int DailyBonusDay;
        public int DailyBonusUsed;
        public int FreeRefreshDay;
    }

    private State _state = new();

    public IReadOnlyList<ContractSlot> Slots => _state.Slots;
    public int Reputation => _state.Reputation;
    public int ClaimedRank => _state.ClaimedRank;

    public static int Today
    {
        get
        {
            var d = DateTime.Now;
            return d.Year * 10000 + d.Month * 100 + d.Day;
        }
    }

    public override void Initialize()
    {
        _state = SaveSystem.Load<State>(SaveKey, null) ?? new State();
        _state.Slots ??= new List<ContractSlot>();
    }

    public int GetDailyBonusUsed() => _state.DailyBonusDay == Today ? _state.DailyBonusUsed : 0;

    public bool IsFreeRefreshAvailable() => _state.FreeRefreshDay != Today;

    public void EnsureSlotCount(int count)
    {
        bool changed = false;
        while (_state.Slots.Count < count) { _state.Slots.Add(new ContractSlot()); changed = true; }
        while (_state.Slots.Count > count) { _state.Slots.RemoveAt(_state.Slots.Count - 1); changed = true; }
        if (changed) Save();
    }

    public void SetSlot(int index, string missionId, int orderClass)
    {
        if (index < 0 || index >= _state.Slots.Count) return;

        _state.Slots[index] = new ContractSlot { MissionId = missionId, OrderClass = orderClass };
        Save();
    }

    public void ClearAllSlots(bool useFreeRefresh)
    {
        foreach (var slot in _state.Slots)
            slot.MissionId = null;

        if (useFreeRefresh)
            _state.FreeRefreshDay = Today;

        Save();
    }

    /// <summary>Contract finished: reputation, daily bonus use, the card leaves the board.</summary>
    public void CompleteContract(int slot, int reputation, bool usedDailyBonus)
    {
        _state.Reputation += Mathf.Max(0, reputation);

        if (usedDailyBonus)
        {
            int today = Today;
            if (_state.DailyBonusDay != today)
            {
                _state.DailyBonusDay = today;
                _state.DailyBonusUsed = 0;
            }
            _state.DailyBonusUsed++;
        }

        if (slot >= 0 && slot < _state.Slots.Count)
            _state.Slots[slot].MissionId = null;

        Save();
    }

    public void SetClaimedRank(int rank)
    {
        _state.ClaimedRank = Mathf.Max(_state.ClaimedRank, rank);
        Save();
    }

    private void Save()
    {
        SaveSystem.Save(SaveKey, _state);
    }
}
