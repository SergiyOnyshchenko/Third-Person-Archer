using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Economy
{
    [CreateAssetMenu(menuName = "Economy/Config/Currency Starting Balances", fileName = "CurrencyStartingBalanceConfig")]
    public class CurrencyStartingBalanceConfig : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public CurrencyType Currency;
            [Min(0)] public int StartAmount;
            public bool UseCap;
            [Min(0)] public int Cap; // Optional soft cap (e.g., to pace progression)
        }

        [SerializeField] private List<Entry> entries = new();

        public int GetStart(CurrencyType type, int fallback = 0)
        {
            foreach (var e in entries)
                if (e.Currency == type) return Mathf.Max(0, e.StartAmount);
            return Mathf.Max(0, fallback);
        }

        public bool TryGetCap(CurrencyType type, out int cap)
        {
            foreach (var e in entries)
                if (e.Currency == type && e.UseCap)
                {
                    cap = Mathf.Max(0, e.Cap);
                    return true;
                }
            cap = 0;
            return false;
        }
    }
}