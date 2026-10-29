using System;
using System.Collections.Generic;

namespace Meta.Weapons
{
    /// <summary>
    /// Tracks ad-assisted upgrades per weapon class per local calendar day.
    /// </summary>
    public sealed class AdUpgradeQuota
    {
        private const string SaveFileName = "AdUpgradeQuota";

        [Serializable]
        private sealed class State
        {
            public string Day;
            public Dictionary<WeaponClass, int> UsedByClass = new();
        }

        private readonly int _perClassPerDay;
        private State _state;

        public AdUpgradeQuota(int perClassPerDay)
        {
            _perClassPerDay = perClassPerDay;
            _state = SaveSystem.Load<State>(SaveFileName) ?? new State();
        }

        public bool HasQuota(WeaponClass cls)
        {
            ResetIfNewDay();
            _state.UsedByClass.TryGetValue(cls, out int used);
            return used < _perClassPerDay;
        }

        public void Consume(WeaponClass cls)
        {
            ResetIfNewDay();
            _state.UsedByClass.TryGetValue(cls, out int used);
            _state.UsedByClass[cls] = used + 1;
            SaveSystem.Save(_state, SaveFileName);
        }

        private void ResetIfNewDay()
        {
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            if (_state.Day == today) return;

            _state.Day = today;
            _state.UsedByClass.Clear();
        }
    }
}
