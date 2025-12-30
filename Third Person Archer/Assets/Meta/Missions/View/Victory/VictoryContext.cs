using System;
using System.Collections.Generic;
using Meta.Economy;

[Serializable]
public sealed class VictoryContext
{
    public MissionType missionType;

    public int zoneIndex;
    public int indexInsideZone;   // campaign index inside current zone (0-based). For non-campaign you can keep 0.

    public int enemiesKilled;

    // Optional: keep if you want to show loop
    public int loopIndex;
}

[Serializable]
public struct CurrencyRewardEntry
{
    public CurrencyType currency;
    public int amount;

    public CurrencyRewardEntry(CurrencyType currency, int amount)
    {
        this.currency = currency;
        this.amount = amount;
    }
}

[Serializable]
public sealed class FinalRewardBundle
{
    public int baseCash;
    public List<CurrencyRewardEntry> baseCurrencies = new();

    public float multiplier = 1f;

    public int finalCash;
    public List<CurrencyRewardEntry> finalCurrencies = new();
}