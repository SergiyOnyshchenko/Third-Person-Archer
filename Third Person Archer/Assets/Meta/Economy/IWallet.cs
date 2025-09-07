using System;

namespace Meta.Economy
{
    /// <summary>
    /// Currency wallet abstraction (domain layer).
    /// </summary>
    public interface IWallet
    {
        event Action<CurrencyType, int, int> BalanceChanged; // (type, newBalance, delta)

        int Get(CurrencyType type);
        bool CanAfford(CurrencyType type, int amount);
        bool TrySpend(CurrencyType type, int amount);
        void Add(CurrencyType type, int amount);
        void Set(CurrencyType type, int amount);

        /// <summary>Reset balances to configured starting values.</summary>
        void ResetToConfig();
    }
}
