using System;
using UnityEngine;

namespace Meta.Energy
{
    [Serializable]
    public sealed class EnergyState
    {
        [SerializeField] private int _currentEnergy;
        [SerializeField] private long _lastEnergyUpdateUnix;   // UTC seconds
        [SerializeField] private int _adRefillsUsedToday;
        [SerializeField] private int _adRefillsDayAnchor;      // yyyyMMdd in UTC day (with reset hour consideration)
        [SerializeField] private int _remainderSeconds;        // carryover toward next point

        public int CurrentEnergy { get => _currentEnergy; set => _currentEnergy = value; }
        public long LastEnergyUpdateUnix { get => _lastEnergyUpdateUnix; set => _lastEnergyUpdateUnix = value; }
        public int AdRefillsUsedToday { get => _adRefillsUsedToday; set => _adRefillsUsedToday = value; }
        public int AdRefillsDayAnchor { get => _adRefillsDayAnchor; set => _adRefillsDayAnchor = value; }
        public int RemainderSeconds { get => _remainderSeconds; set => _remainderSeconds = value; }

        public static EnergyState CreateDefault(EnergyConfig config, long nowUtcSeconds)
        {
            return new EnergyState
            {
                _currentEnergy = Mathf.Clamp(config.StartingEnergy, 0, config.MaxEnergy),
                _lastEnergyUpdateUnix = nowUtcSeconds,
                _adRefillsUsedToday = 0,
                _adRefillsDayAnchor = DateKeyUtil.UtcDateKey(nowUtcSeconds, config.DailyResetHourUtc),
                _remainderSeconds = 0
            };
        }
    }

    internal static class DateKeyUtil
    {
        // UTC day key with custom reset hour (0 = midnight UTC).
        public static int UtcDateKey(long unixSeconds, int resetHourUtc)
        {
            var dt = DateTimeOffset.FromUnixTimeSeconds(unixSeconds).UtcDateTime;
            if (resetHourUtc > 0)
            {
                // If current hour is before reset, count as previous day.
                if (dt.Hour < resetHourUtc) dt = dt.AddDays(-1);
            }
            var y = dt.Year;
            var m = dt.Month;
            var d = dt.Day;
            return y * 10000 + m * 100 + d; // yyyyMMdd
        }
    }
}