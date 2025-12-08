using UnityEngine;

namespace Meta.Weapons
{
    public class StatsService : IStatsService
    {
        public WeaponStats Compute(WeaponDef def, int upgradeLevel)
        {
            if (def == null) return WeaponStats.Zero;

            var tempInstance = new WeaponInstance
            {
                WeaponId = def.Id,
                Owned = true,
                UpgradeLevel = upgradeLevel
            };

            return Compute(def, tempInstance);
        }

        public WeaponStats Compute(WeaponDef def, WeaponInstance instance)
        {
            if (def == null) return WeaponStats.Zero;
            if (instance == null) return def.BaseStats;

            // No parts/mastery anymore: just interpolate between BaseStats and MaxStats using curve.
            var baseStats = def.BaseStats;
            var maxStats = def.MaxStats;

            if (def.MaxUpgradeLevel <= 0)
                return baseStats;

            float tNorm = def.GetNormalizedLevel(instance.UpgradeLevel);
            float tCurve = def.UpgradeCurve01 != null
                ? def.UpgradeCurve01.Evaluate(tNorm)
                : tNorm;

            tCurve = Mathf.Clamp01(tCurve);

            return LerpWeaponStats(baseStats, maxStats, tCurve);
        }

        private static WeaponStats LerpWeaponStats(WeaponStats a, WeaponStats b, float t)
        {
            return new WeaponStats
            {
                Damage = Mathf.Lerp(a.Damage, b.Damage, t),
                Balance = Mathf.Lerp(a.Balance, b.Balance, t),
                Distance = Mathf.Lerp(a.Distance, b.Distance, t),
                AmmoCount = Mathf.Lerp(a.AmmoCount, b.AmmoCount, t),
                ReloadTime = Mathf.Lerp(a.ReloadTime, b.ReloadTime, t),
                Zoom = Mathf.Lerp(a.Zoom, b.Zoom, t),
            };
        }
    }
}