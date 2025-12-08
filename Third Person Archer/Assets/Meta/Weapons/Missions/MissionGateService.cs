using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Meta.Weapons
{
    public class MissionGateService : IMissionGateService
    {
        private readonly IWeaponRepository _repo;
        private readonly IStatsService _stats;
        private readonly WeaponDef[] _allDefs;

        public MissionGateService(IWeaponRepository repo, IStatsService stats, WeaponDef[] allDefs)
        {
            _repo = repo;
            _stats = stats;
            _allDefs = allDefs;
        }

        public MissionGateResult Evaluate(MissionRequirement req)
        {
            return null;
        }
        
        private static bool Beats(WeaponStats candidate, WeaponStats baseline, List<WeaponStat> shortfalls, MissionRequirement r)
        {
            // Candidate is better if it resolves MOST shortfalls and does not become worse on reload max constraint, etc.
            var resolved = 0;
            foreach (var st in shortfalls)
            {
                switch (st)
                {
                    case WeaponStat.ReloadTime:
                        if (r.ReloadTimeIsMaxNotMin && candidate.ReloadTime <= r.RecommendedMinimum.ReloadTime && candidate.ReloadTime < baseline.ReloadTime)
                            resolved++;
                        else if (!r.ReloadTimeIsMaxNotMin && candidate.ReloadTime >= r.RecommendedMinimum.ReloadTime && candidate.ReloadTime > baseline.ReloadTime)
                            resolved++;
                        break;
                    default:
                        var cand = Get(candidate, st);
                        var rec = Get(r.RecommendedMinimum, st);
                        var baseVal = Get(baseline, st);
                        if (cand >= rec && cand > baseVal) resolved++;
                        break;
                }
            }
            return resolved >= Mathf.Max(1, shortfalls.Count); // resolves all or most
        }

        private static float Get(WeaponStats s, WeaponStat st)
        {
            return st switch
            {
                WeaponStat.Damage => s.Damage,
                WeaponStat.Balance => s.Balance,
                WeaponStat.Distance => s.Distance,
                WeaponStat.AmmoCount => s.AmmoCount,
                WeaponStat.ReloadTime => s.ReloadTime,
                WeaponStat.Zoom => s.Zoom,
                _ => 0
            };
        }
    }
}
