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
            var state = _repo.Load();

            // Find equipped of required class
            var equipped = state.Weapons.FirstOrDefault(w =>
            {
                if (!w.Equipped || !w.Owned) return false;
                var def = _allDefs.FirstOrDefault(d => d.Id == w.WeaponId);
                return def != null && def.Class == req.RequiredClass;
            });

            if (equipped == null)
            {
                // If none equipped, but owned exists, suggest one
                var ownedAny = state.Weapons
                    .Where(w => w.Owned)
                    .Select(w => _allDefs.FirstOrDefault(d => d.Id == w.WeaponId))
                    .Where(d => d != null && d.Class == req.RequiredClass)
                    .Select(d => d.Id)
                    .ToArray();

                return new MissionGateResult
                {
                    Status = ownedAny.Length > 0 ? GateStatus.Warn : GateStatus.Block,
                    EquippedWeaponId = null,
                    SuggestedAlternativeWeaponIds = ownedAny
                };
            }

            var defEq = _allDefs.First(d => d.Id == equipped.WeaponId);
            var statsEq = _stats.Compute(defEq, equipped);

            var ok = MeetsRecommended(statsEq, req, out var shortfalls);
            if (ok)
            {
                return new MissionGateResult
                {
                    Status = GateStatus.Ok,
                    EquippedWeaponId = equipped.WeaponId,
                    SuggestedUpgradesPartIds = System.Array.Empty<string>(),
                    SuggestedAlternativeWeaponIds = System.Array.Empty<string>()
                };
            }

            // Suggest parts that impact the short stats the most
            var suggestions = RankPartsByImpact(defEq, equipped, shortfalls, _stats)
                .Take(3)
                .Select(p => p.Id)
                .ToArray();

            // If owned alternatives exist with better stats, list them
            var betterOwned = state.Weapons
                .Where(w => w.Owned && w.WeaponId != equipped.WeaponId)
                .Select(w => new { w, d = _allDefs.FirstOrDefault(x => x.Id == w.WeaponId) })
                .Where(x => x.d != null && x.d.Class == defEq.Class)
                .Select(x => new { x.d.Id, s = _stats.Compute(x.d, x.w) })
                .Where(x => Beats(x.s, statsEq, shortfalls, req))
                .Select(x => x.Id)
                .Take(3)
                .ToArray();

            return new MissionGateResult
            {
                Status = GateStatus.Warn,
                EquippedWeaponId = equipped.WeaponId,
                SuggestedUpgradesPartIds = suggestions,
                SuggestedAlternativeWeaponIds = betterOwned
            };
        }

        private static bool MeetsRecommended(WeaponStats s, MissionRequirement r, out List<WeaponStat> shortfalls)
        {
            const float EPS = 1e-4f;
            shortfalls = new List<WeaponStat>();

            // "Min" style stats (higher is better)
            if (s.Damage    + EPS < r.RecommendedMinimum.Damage)    shortfalls.Add(WeaponStat.Damage);
            if (s.Balance   + EPS < r.RecommendedMinimum.Balance)   shortfalls.Add(WeaponStat.Balance);
            if (s.Distance  + EPS < r.RecommendedMinimum.Distance)  shortfalls.Add(WeaponStat.Distance);
            if (s.AmmoCount + EPS < r.RecommendedMinimum.AmmoCount) shortfalls.Add(WeaponStat.AmmoCount);
            if (s.Zoom      + EPS < r.RecommendedMinimum.Zoom)      shortfalls.Add(WeaponStat.Zoom);

            // ReloadTime can be "max" or "min" constrained depending on your mission rule
            if (r.ReloadTimeIsMaxNotMin)
            {
                // Lower is better; flag if we're slower (greater) than the recommended max
                if (s.ReloadTime - EPS > r.RecommendedMinimum.ReloadTime)
                    shortfalls.Add(WeaponStat.ReloadTime);
            }
            else
            {
                // Higher is better; flag if we're below the recommended min
                if (s.ReloadTime + EPS < r.RecommendedMinimum.ReloadTime)
                    shortfalls.Add(WeaponStat.ReloadTime);
            }

            return shortfalls.Count == 0;
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

        private static IEnumerable<WeaponPartDef> RankPartsByImpact(WeaponDef def, WeaponInstance inst, List<WeaponStat> shortStats, IStatsService stats)
        {
            // naive heuristic: try each candidate part's NEXT level and see the delta in short stats
            var current = stats.Compute(def, inst);
            var scored = new List<(WeaponPartDef part, float score)>();

            foreach (var part in def.Parts)
            {
                inst.PartLevels.TryGetValue(part.Id, out var lvl);
                if (lvl >= part.MaxLevel) continue; // only suggest until cap

                var next = lvl + 1;
                var nextInst = Clone(inst);
                nextInst.PartLevels[part.Id] = next;
                var nextStats = stats.Compute(def, nextInst);

                float score = 0f;
                foreach (var st in shortStats)
                {
                    var before = Get(current, st);
                    var after = Get(nextStats, st);
                    // ReloadTime: lower is better
                    if (st == WeaponStat.ReloadTime)
                        score += Mathf.Max(0f, before - after);
                    else
                        score += Mathf.Max(0f, after - before);
                }

                if (score > 0f) scored.Add((part, score));
            }

            return scored.OrderByDescending(x => x.score).Select(x => x.part);

            static WeaponInstance Clone(WeaponInstance src)
            {
                return new WeaponInstance
                {
                    WeaponId = src.WeaponId,
                    Owned = src.Owned,
                    Equipped = src.Equipped,
                    PartLevels = new Dictionary<string, int>(src.PartLevels),
                    MasteryTiers = new Dictionary<string, int>(src.MasteryTiers)
                };
            }
        }
    }
}
