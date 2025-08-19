using System;
using UnityEngine;

namespace Meta.Energy
{
    public enum SpendResult { Success, NotEnough }
    public enum AdRefillResult { Granted, CappedForToday, FullAlready, NotAvailable, Error }
    public enum PurchaseResult { Granted, NotEnoughGold, Error }

    public interface IWalletService
    {
        bool TrySpendGold(int amount);
        int CurrentGold { get; }
    }

    public interface IAdsService
    {
        bool IsRewardedAvailable(out string reason);
        // Showing the ad is outside this service; call ApplyAdRefillGrant() after completion.
    }

    public sealed class EnergyService
    {
        private const string SaveFileName = "EnergyState";

        private readonly EnergyConfig _config;
        private readonly ITimeProvider _time;
        private EnergyState _state;

        private int _secondsToNextPoint; // 0 when full
        private float _tickAccumulator;  // handle float deltaTimes

        // Events for UI/analytics hooks
        public event Action<int, int> OnEnergyChanged;
        public event Action<int> OnSecondsToNextChanged;
        public event Action OnDailyReset;

        public int CurrentEnergy => _state?.CurrentEnergy ?? 0;
        public int MaxEnergy => _config.MaxEnergy;
        public bool IsFull => CurrentEnergy >= _config.MaxEnergy;
        public int SecondsToNextPoint => _secondsToNextPoint;
        public int AdRefillsRemaining => Mathf.Max(0, _config.MaxAdRefillsPerDay - _state.AdRefillsUsedToday);

        public EnergyService(EnergyConfig config, ITimeProvider timeProvider)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _time = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
            LoadOrBootstrap();
            RecalculateFromNow();
        }

        // ---------- Public API ----------

        /// <summary>Rebuild regen based on local UTC time (and apply daily reset).</summary>
        public void RecalculateFromNow()
        {
            if (_state == null) return;

            var now = _time.UtcNowSeconds;
            var dayKey = DateKeyUtil.UtcDateKey(now, _config.DailyResetHourUtc);
            if (dayKey != _state.AdRefillsDayAnchor)
            {
                _state.AdRefillsDayAnchor = dayKey;
                _state.AdRefillsUsedToday = 0;
                Save();
                OnDailyReset?.Invoke();
            }

            ApplyRegen(now);
        }

        /// <summary>Foreground 1s ticking for countdown + natural regen.</summary>
        public void TickForeground(float deltaSeconds)
        {
            if (IsFull) return;
            if (_secondsToNextPoint <= 0) return;

            _tickAccumulator += deltaSeconds;
            while (_tickAccumulator >= 1f)
            {
                _tickAccumulator -= 1f;

                _secondsToNextPoint = Mathf.Max(0, _secondsToNextPoint - 1);
                OnSecondsToNextChanged?.Invoke(_secondsToNextPoint);

                if (_secondsToNextPoint == 0)
                {
                    // Award 1 energy
                    _state.CurrentEnergy = Mathf.Min(_config.MaxEnergy, _state.CurrentEnergy + 1);
                    _state.LastEnergyUpdateUnix = _time.UtcNowSeconds;
                    _state.RemainderSeconds = 0;

                    OnEnergyChanged?.Invoke(_state.CurrentEnergy, _config.MaxEnergy);

                    if (_state.CurrentEnergy < _config.MaxEnergy)
                    {
                        _secondsToNextPoint = _config.RegenIntervalSeconds;
                        OnSecondsToNextChanged?.Invoke(_secondsToNextPoint);
                    }
                    Save();

                    if (IsFull) return; // stop ticking
                }
            }
        }

        /// <summary>
        /// Attempt to spend energy based on the configured cost for the given mission type.
        /// 0 cost types are treated as free and succeed without modifying energy.
        /// </summary>
        public SpendResult TrySpendForMission(MissionType missionType)
        {
            int cost = Mathf.Max(0, _config.GetCostForType(missionType));
            return TrySpendInternal(cost);
        }

        /// <summary>
        /// Attempt to spend an explicit energy amount (useful for special missions or events).
        /// </summary>
        public SpendResult TrySpend(int explicitCost)
        {
            int cost = Mathf.Max(0, explicitCost);
            return TrySpendInternal(cost);
        }

        /// <summary>Quick check for UI gating before calling TrySpend().</summary>
        public bool CanSpend(MissionType missionType)
        {
            int cost = Mathf.Max(0, _config.GetCostForType(missionType));
            if (cost == 0) return true;
            return CurrentEnergy >= cost;
        }

        /// <summary>Expose the configured energy cost for a mission type (for UI display).</summary>
        public int GetCostFor(MissionType missionType) => Mathf.Max(0, _config.GetCostForType(missionType));

        public bool CanAdRefill(out string reason)
        {
            if (IsFull)
            {
                reason = "Energy is full";
                return false;
            }
            if (_state.AdRefillsUsedToday >= _config.MaxAdRefillsPerDay)
            {
                reason = "Daily ad refills reached";
                return false;
            }

            reason = null;
            return true;
        }

        /// <summary>Call after a rewarded ad completes to grant energy.</summary>
        public AdRefillResult ApplyAdRefillGrant()
        {
            if (IsFull) return AdRefillResult.FullAlready;
            if (_state.AdRefillsUsedToday >= _config.MaxAdRefillsPerDay) return AdRefillResult.CappedForToday;

            var grant = Mathf.Min(_config.AdRefillAmount, _config.MaxEnergy - _state.CurrentEnergy);
            if (grant <= 0) return AdRefillResult.FullAlready;

            _state.CurrentEnergy += grant;
            _state.AdRefillsUsedToday++;

            if (IsFull)
            {
                _secondsToNextPoint = 0;
                _state.RemainderSeconds = 0;
            }
            else if (_secondsToNextPoint <= 0)
            {
                _secondsToNextPoint = _config.RegenIntervalSeconds;
            }

            _state.LastEnergyUpdateUnix = _time.UtcNowSeconds;

            OnEnergyChanged?.Invoke(_state.CurrentEnergy, _config.MaxEnergy);
            OnSecondsToNextChanged?.Invoke(_secondsToNextPoint);

            Save();
            return AdRefillResult.Granted;
        }

        public PurchaseResult RefillToMaxByGold(IWalletService wallet)
        {
            if (wallet == null) return PurchaseResult.Error;
            if (IsFull) return PurchaseResult.Granted;

            if (!wallet.TrySpendGold(_config.EnergyRefillCostGold))
                return PurchaseResult.NotEnoughGold;

            _state.CurrentEnergy = _config.MaxEnergy;
            _secondsToNextPoint = 0;
            _state.RemainderSeconds = 0;
            _state.LastEnergyUpdateUnix = _time.UtcNowSeconds;

            OnEnergyChanged?.Invoke(_state.CurrentEnergy, _config.MaxEnergy);
            OnSecondsToNextChanged?.Invoke(_secondsToNextPoint);

            Save();
            return PurchaseResult.Granted;
        }

        public void ApplyFullRefillReward(string source = null)
        {
            if (IsFull) return;

            _state.CurrentEnergy = _config.MaxEnergy;
            _secondsToNextPoint = 0;
            _state.RemainderSeconds = 0;
            _state.LastEnergyUpdateUnix = _time.UtcNowSeconds;

            OnEnergyChanged?.Invoke(_state.CurrentEnergy, _config.MaxEnergy);
            OnSecondsToNextChanged?.Invoke(_secondsToNextPoint);

            Save();
        }

        public void ForceSave() => Save();

        // ---------- Internals ----------

        /// <summary>
        /// Centralized spend logic to avoid duplication across TrySpend* entry points.
        /// Handles: recalc, zero-cost fast-path, atomic decrement, timer bootstrap, events, and save.
        /// </summary>
        private SpendResult TrySpendInternal(int cost)
        {
            RecalculateFromNow();

            if (cost <= 0)
                return SpendResult.Success;

            if (_state.CurrentEnergy >= cost)
            {
                _state.CurrentEnergy -= cost;
                _state.LastEnergyUpdateUnix = _time.UtcNowSeconds;

                // If we just left the "full" state or no timer is running, start the countdown.
                if (!IsFull && _secondsToNextPoint == 0)
                {
                    _secondsToNextPoint = _config.RegenIntervalSeconds;
                    OnSecondsToNextChanged?.Invoke(_secondsToNextPoint);
                }

                OnEnergyChanged?.Invoke(_state.CurrentEnergy, _config.MaxEnergy);
                Save();
                return SpendResult.Success;
            }

            return SpendResult.NotEnough;
        }

        private void LoadOrBootstrap()
        {
            var loaded = SaveSystem.Load<EnergyState>(SaveFileName, default);
            var now = _time.UtcNowSeconds;

            if (loaded == null)
            {
                _state = EnergyState.CreateDefault(_config, now);
                Save();
            }
            else
            {
                _state = loaded;
                // Safety clamp if config changed since last version
                _state.CurrentEnergy = Mathf.Clamp(_state.CurrentEnergy, 0, _config.MaxEnergy);
            }
        }

        private void ApplyRegen(long now)
        {
            // Already full → just zero the countdown.
            if (_state.CurrentEnergy >= _config.MaxEnergy)
            {
                _state.RemainderSeconds = 0;
                _secondsToNextPoint = 0;
                _state.LastEnergyUpdateUnix = now; // normalize timestamp
                Save();
                OnSecondsToNextChanged?.Invoke(_secondsToNextPoint);
                return;
            }

            var elapsed = now - _state.LastEnergyUpdateUnix;

            if (elapsed < 0)
            {
                // Clock rollback; tolerate small skew if configured
                if (Mathf.Abs((int)elapsed) <= _config.ClockSkewToleranceSeconds)
                {
                    elapsed = 0;
                }
                else
                {
                    // Large negative skew → ignore regen this time
                    elapsed = 0;
                }
            }

            long total = Mathf.Max(0, (int)elapsed) + _state.RemainderSeconds;
            if (total <= 0)
            {
                // Just rebuild next timer if needed
                _secondsToNextPoint = _state.RemainderSeconds > 0
                    ? _config.RegenIntervalSeconds - _state.RemainderSeconds
                    : Mathf.Max(0, _secondsToNextPoint);
                OnSecondsToNextChanged?.Invoke(_secondsToNextPoint);
                return;
            }

            int points = (int)(total / _config.RegenIntervalSeconds);
            int remainder = (int)(total % _config.RegenIntervalSeconds);

            if (points > 0)
            {
                _state.CurrentEnergy = Mathf.Min(_config.MaxEnergy, _state.CurrentEnergy + points);
                OnEnergyChanged?.Invoke(_state.CurrentEnergy, _config.MaxEnergy);
            }

            if (_state.CurrentEnergy >= _config.MaxEnergy)
            {
                _state.RemainderSeconds = 0;
                _secondsToNextPoint = 0;
            }
            else
            {
                _state.RemainderSeconds = remainder;
                _secondsToNextPoint = _config.RegenIntervalSeconds - remainder;
            }

            _state.LastEnergyUpdateUnix = now;
            OnSecondsToNextChanged?.Invoke(_secondsToNextPoint);
            Save();
        }

        private void Save()
        {
            SaveSystem.Save(SaveFileName, _state);
        }
    }
}
