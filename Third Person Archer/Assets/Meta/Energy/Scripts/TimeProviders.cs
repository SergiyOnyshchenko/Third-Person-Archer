using System;

namespace Meta.Energy
{
    public interface ITimeProvider
    {
        long UtcNowSeconds { get; }
    }

    public sealed class LocalTimeProvider : ITimeProvider
    {
        public long UtcNowSeconds => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }
}