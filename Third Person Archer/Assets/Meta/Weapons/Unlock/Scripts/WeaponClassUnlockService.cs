using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Meta.Weapons.Unlocks
{
    public sealed class WeaponClassUnlockService : IWeaponClassUnlockService
    {
        private readonly WeaponClassUnlockConfig _config;
        private readonly IWeaponClassUnlockRepository _repo;
        private WeaponClassUnlockState _state;

        public WeaponClassUnlockService(WeaponClassUnlockConfig config, IWeaponClassUnlockRepository repo)
        {
            _config = config;
            _repo = repo;
            _state = _repo.Load();
            BootstrapDefaults();
        }

        private void BootstrapDefaults()
        {
            foreach (var rule in _config.EnumerateOrdered())
            {
                var e = _state.GetOrCreate(rule.Class);
                if (rule.StartsUnlocked && !e.Unlocked)
                {
                    e.Unlocked = true;
                }
            }
            _repo.Save(_state);
        }

        public bool IsUnlocked(WeaponClass cls)
        {
            var e = _state.GetOrCreate(cls);
            return e.Unlocked;
        }

        public ClassProgress GetProgress(WeaponClass cls)
        {
            if (!_config.TryGetRule(cls, out var rule))
                return new ClassProgress(cls, 0, 1, IsUnlocked(cls)); // unknown rule: treat as locked or unlocked by state

            var e = _state.GetOrCreate(cls);
            int required = Mathf.Max(1, rule.RequiredCount);
            int current = Mathf.Clamp(IntersectCount(e.CreditedMissions, rule.MissionIds), 0, required);
            return new ClassProgress(cls, current, required, e.Unlocked);
        }

        public IReadOnlyList<ClassProgressDelta> RegisterMissionComplete(string missionId)
        {
            if (string.IsNullOrEmpty(missionId)) return System.Array.Empty<ClassProgressDelta>();

            var deltas = new List<ClassProgressDelta>();
            bool dirty = false;

            foreach (var rule in _config.EnumerateOrdered())
            {
                // If the mission contributes to this class
                if (rule.MissionIds == null || !rule.MissionIds.Contains(missionId)) continue;

                var entry = _state.GetOrCreate(rule.Class);
                var before = GetProgress(rule.Class);

                // Add credit once
                if (!entry.CreditedMissions.Contains(missionId))
                {
                    entry.CreditedMissions.Add(missionId);
                    dirty = true;
                }

                // Check unlock
                var afterTmp = GetProgress(rule.Class);
                if (!entry.Unlocked && afterTmp.Current >= afterTmp.Required)
                {
                    entry.Unlocked = true;
                    dirty = true;
                }

                var after = GetProgress(rule.Class);
                // Only include meaningful deltas
                if (after.Current != before.Current || after.Unlocked != before.Unlocked)
                    deltas.Add(new ClassProgressDelta(before, after));
            }

            if (dirty) _repo.Save(_state);
            return deltas;
        }

        public WeaponClass? GetNextLockedClass()
        {
            foreach (var r in _config.EnumerateOrdered())
            {
                if (!IsUnlocked(r.Class))
                    return r.Class;
            }
            return null;
        }

        private static int IntersectCount(List<string> a, List<string> b)
        {
            if (a == null || b == null) return 0;
            int count = 0;
            // small lists → simple scan
            foreach (var id in a)
                if (b.Contains(id)) count++;
            return count;
        }
    }
}