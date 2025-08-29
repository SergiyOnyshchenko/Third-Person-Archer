using System;
using Meta.Economy;

namespace Meta.Weapons
{
    public interface IWalletService
    {
        bool CanAfford(CurrencyType currency, int amount);
        void Spend(CurrencyType currency, int amount);
        int GetBalance(CurrencyType currency);
    }

    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }

    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}