using System;
using Meta.Economy;

namespace Meta.Weapons
{
    public interface ITimeProvider
    {
        DateTime UtcNow { get; }
    }

    public class SystemTimeProvider : ITimeProvider
    {
        public DateTime UtcNow => DateTime.UtcNow;
    }
}