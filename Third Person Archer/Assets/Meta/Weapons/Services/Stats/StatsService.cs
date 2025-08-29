using UnityEngine;

namespace Meta.Weapons
{
    public class StatsService : IStatsService
    {
        public WeaponStats Compute(WeaponDef def, WeaponInstance instance)
        {
            if (def == null) return WeaponStats.Zero;

            var stats = def.BaseStats;

            // Apply part levels
            foreach (var part in def.Parts)
            {
                instance.PartLevels.TryGetValue(part.Id, out var level);
                if (level <= 0) continue;

                // clamp
                level = Mathf.Clamp(level, 1, part.MaxLevel);

                var lvlDef = part.Levels[level - 1];
                if (lvlDef.Modifiers != null)
                {
                    for (int i = 0; i < lvlDef.Modifiers.Length; i++)
                        lvlDef.Modifiers[i].Apply(ref stats);
                }
            }

            // Apply Mastery (percent modifiers)
            foreach (var part in def.Parts)
            {
                if (part.Mastery == null) continue;
                instance.MasteryTiers.TryGetValue(part.Id, out var tier);
                if (tier <= 0) continue;

                tier = Mathf.Clamp(tier, 1, part.Mastery.MaxTier);
                var t = part.Mastery.Tiers[tier - 1];
                if (t.PercentModifiers != null)
                {
                    for (int i = 0; i < t.PercentModifiers.Length; i++)
                        t.PercentModifiers[i].Apply(ref stats);
                }
            }

            return stats;
        }
    }
}