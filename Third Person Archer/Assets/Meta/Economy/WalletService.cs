using System;
using System.Collections.Generic;

namespace Meta.Economy
{
    /// <summary>
    /// Application service implementing wallet rules (Single Responsibility).
    /// Depends on storage + config (Dependency Inversion).
    /// </summary>
    public sealed class WalletService : IWallet
    {
        public event Action<CurrencyType, int, int> BalanceChanged;

        private readonly ICurrencyStorage _storage;
        private readonly CurrencyStartingBalanceConfig _config;
        private readonly Dictionary<CurrencyType, int> _cache = new();

        public WalletService(ICurrencyStorage storage, CurrencyStartingBalanceConfig config)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _config = config ?? throw new ArgumentNullException(nameof(config));

            foreach (CurrencyType t in Enum.GetValues(typeof(CurrencyType)))
            {
                int start = _config.GetStart(t, 0);
                int val = _storage.Load(t, start);
                _cache[t] = ClampToCap(t, Math.Max(0, val));
            }
        }

        public int Get(CurrencyType type) => _cache[type];

        public bool CanAfford(CurrencyType type, int amount)
        {
            if (amount <= 0) return true;
            return _cache[type] >= amount;
        }

        public bool TrySpend(CurrencyType type, int amount)
        {
            if (amount <= 0) return true;
            if (!CanAfford(type, amount)) return false;

            var before = _cache[type];
            var after = before - amount;
            Apply(type, after, after - before);
            return true;
        }

        public void Add(CurrencyType type, int amount)
        {
            if (amount == 0) return;
            var before = _cache[type];
            var after = ClampToCap(type, before + amount);
            Apply(type, after, after - before);
        }

        public void Set(CurrencyType type, int amount)
        {
            amount = Math.Max(0, ClampToCap(type, amount));
            var before = _cache[type];
            if (before == amount) return;
            Apply(type, amount, amount - before);
        }

        public void ResetToConfig()
        {
            foreach (CurrencyType t in Enum.GetValues(typeof(CurrencyType)))
                Set(t, _config.GetStart(t, 0));
        }

        private void Apply(CurrencyType type, int newValue, int delta)
        {
            _cache[type] = Math.Max(0, newValue);
            _storage.Save(type, _cache[type]);
            BalanceChanged?.Invoke(type, _cache[type], delta);
        }

        private int ClampToCap(CurrencyType type, int value)
        {
            if (_config.TryGetCap(type, out var cap))
                return Math.Min(value, cap);
            return value;
        }
    }
}
