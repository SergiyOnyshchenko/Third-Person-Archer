using UnityEngine;
using System.Collections.Generic;

namespace Meta.Energy
{
    [CreateAssetMenu(fileName = "EnergyConfig", menuName = "Game/Energy/EnergyConfig")]
    public sealed class EnergyConfig : ScriptableObject
    {
        [System.Serializable]
        public struct EnergyCostByType
        {
            public MissionType Type;
            [Min(0)] public int Cost;
        }

        [Header("Meta")] [SerializeField, Min(1)]
        private int _version = 1;

        [SerializeField] private bool _enableDesignerWarnings = true;
        [SerializeField, TextArea(2, 6)] private string _notes = string.Empty;

        [Header("Tuning")] [SerializeField, Min(0)]
        private int _startingEnergy = 10;

        [SerializeField, Min(1)] private int _maxEnergy = 10;
        [SerializeField, Range(30, 86400)] private int _regenIntervalSeconds = 300;
        [SerializeField, Min(0)] private int _defaultEnergyCost = 15;
        [SerializeField] private List<EnergyCostByType> _energyCostsByType = new();

        [Header("Monetization")] [SerializeField, Min(1)]
        private int _energyRefillCostGold = 50;

        [SerializeField, Min(1)] private int _adRefillAmount = 2;
        [SerializeField, Range(0, 50)] private int _maxAdRefillsPerDay = 3;

        [Header("Daily Reset / Clock")] [SerializeField, Range(0, 23)]
        private int _dailyResetHourUtc = 0;

        [SerializeField, Range(0, 3600)] private int _clockSkewToleranceSeconds = 0;

        // Public read-only accessors
        public int Version => _version;
        public bool EnableDesignerWarnings => _enableDesignerWarnings;
        public string Notes => _notes;

        public int StartingEnergy => _startingEnergy;
        public int MaxEnergy => _maxEnergy;
        public int RegenIntervalSeconds => _regenIntervalSeconds;

        public int GetCostForType(MissionType type)
        {
            // Exact override
            for (int i = 0; i < _energyCostsByType.Count; i++)
                if (_energyCostsByType[i].Type == type)
                    return Mathf.Max(0, _energyCostsByType[i].Cost);

            // Default
            return Mathf.Max(0, _defaultEnergyCost);
        }

        public int EnergyRefillCostGold => _energyRefillCostGold;
        public int AdRefillAmount => _adRefillAmount;
        public int MaxAdRefillsPerDay => _maxAdRefillsPerDay;

        public int DailyResetHourUtc => _dailyResetHourUtc;
        public int ClockSkewToleranceSeconds => _clockSkewToleranceSeconds;

#if UNITY_EDITOR
        public string[] GetValidationErrors()
        {
            var list = new System.Collections.Generic.List<string>();

            if (_startingEnergy > _maxEnergy)
                list.Add("StartingEnergy must be <= MaxEnergy.");

            if (_adRefillAmount > _maxEnergy)
                list.Add("AdRefillAmount should not exceed MaxEnergy.");

            if (_regenIntervalSeconds < 30)
                list.Add("RegenIntervalSeconds < 30s is rarely healthy for pacing.");

            if (_energyRefillCostGold < 5 && _enableDesignerWarnings)
                list.Add("EnergyRefillCostGold seems very low; consider >= 5.");

            if (_maxAdRefillsPerDay > 10 && _enableDesignerWarnings)
                list.Add("MaxAdRefillsPerDay > 10 can impact monetization/retention.");

            // New: per-type validation
            var seen = new System.Collections.Generic.HashSet<MissionType>();
            for (int i = 0; i < _energyCostsByType.Count; i++)
            {
                var entry = _energyCostsByType[i];
                if (entry.Cost < 0)
                    list.Add($"Energy cost for {entry.Type} is negative; must be >= 0.");

                if (!seen.Add(entry.Type))
                    list.Add($"Duplicate energy cost entry for {entry.Type}; only the first will be used.");
            }

            if (_defaultEnergyCost < 0)
                list.Add("Default energy cost must be >= 0.");

            return list.ToArray();
        }
#endif

    }
}