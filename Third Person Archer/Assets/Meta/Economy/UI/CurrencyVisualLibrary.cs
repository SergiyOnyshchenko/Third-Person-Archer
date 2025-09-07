using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Economy
{
    /// <summary>
    /// Maps CurrencyType -> display name and icon, so UI can stay generic.
    /// Example: map CurrencyType.Cash to "Coins" with a coin icon.
    /// </summary>
    [CreateAssetMenu(menuName = "Economy/UI/Currency Visual Library", fileName = "CurrencyVisualLibrary")]
    public class CurrencyVisualLibrary : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public CurrencyType Currency;
            public string DisplayName;   // e.g., "Coins", "Gold", "Tags"
            public Sprite Icon;
        }

        [SerializeField] private List<Entry> entries = new();

        public bool TryGet(CurrencyType currency, out string displayName, out Sprite icon)
        {
            foreach (var e in entries)
            {
                if (e.Currency == currency)
                {
                    displayName = string.IsNullOrEmpty(e.DisplayName) ? currency.ToString() : e.DisplayName;
                    icon = e.Icon;
                    return true;
                }
            }
            displayName = currency.ToString();
            icon = null;
            return false;
        }
    }
}