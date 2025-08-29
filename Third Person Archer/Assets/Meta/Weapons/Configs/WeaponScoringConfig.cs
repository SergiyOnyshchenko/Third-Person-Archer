using System;
using UnityEngine;

namespace Meta.Weapons
{
    [CreateAssetMenu(menuName = "Game/Weapons/Scoring Config", fileName = "WeaponScoringConfig")]
    public class WeaponScoringConfig : ScriptableObject
    {
        [Serializable] public struct StatRange { public float Min; public float Max; }

        [Header("Normalization Ranges")]
        public StatRange Damage = new(){ Min = 0,    Max = 5000 };
        public StatRange Balance = new(){ Min = 0,   Max = 100  };
        public StatRange Distance = new(){ Min = 0,  Max = 500  };
        public StatRange AmmoCount = new(){ Min = 0, Max = 30   };
        public StatRange ReloadTime = new(){ Min = 0.5f, Max = 5.0f }; // lower is better
        public StatRange Zoom = new(){ Min = 1.0f, Max = 20.0f  };

        [Header("Base Weights")]
        public float DamageWeight = 1.0f;
        public float BalanceWeight = 0.5f;
        public float DistanceWeight = 0.8f;
        public float AmmoCountWeight = 0.4f;
        public float ReloadTimeWeight = 0.7f; // contributes as (1 - normalized)
        public float ZoomWeight = 0.2f;

        [Header("Requirement Penalties (when below recommendation)")]
        public float ShortfallPenaltyPerStat = 0.75f;

        public float Normalize(WeaponStat stat, float value)
        {
            var r = stat switch
            {
                WeaponStat.Damage     => Damage,
                WeaponStat.Balance    => Balance,
                WeaponStat.Distance   => Distance,
                WeaponStat.AmmoCount  => AmmoCount,
                WeaponStat.ReloadTime => ReloadTime,
                WeaponStat.Zoom       => Zoom,
                _ => new StatRange { Min = 0, Max = 1 }
            };
            if (Mathf.Abs(r.Max - r.Min) < 1e-6f) return 0f;

            float t = Mathf.InverseLerp(r.Min, r.Max, value);
            if (stat == WeaponStat.ReloadTime) t = 1f - t; // lower is better
            return Mathf.Clamp01(t);
        }

        public float ScoreBase(WeaponStats s)
        {
            float dmg  = Normalize(WeaponStat.Damage,     s.Damage)     * DamageWeight;
            float bal  = Normalize(WeaponStat.Balance,    s.Balance)    * BalanceWeight;
            float dist = Normalize(WeaponStat.Distance,   s.Distance)   * DistanceWeight;
            float ammo = Normalize(WeaponStat.AmmoCount,  s.AmmoCount)  * AmmoCountWeight;
            float rld  = Normalize(WeaponStat.ReloadTime, s.ReloadTime) * ReloadTimeWeight; // inverted above
            float zoom = Normalize(WeaponStat.Zoom,       s.Zoom)       * ZoomWeight;
            return dmg + bal + dist + ammo + rld + zoom;
        }

        public float ScoreWithRequirement(WeaponStats s, MissionRequirement req)
        {
            var baseScore = ScoreBase(s);
            if (req == null) return baseScore;

            float penalty = 0f;

            void AddMinPenalty(WeaponStat stat, float value, float min)
            {
                if (value + 1e-4f < min)
                {
                    // how far below, normalized to range
                    var shortfall = (min - value) / Mathf.Max(1e-3f, GetRange(stat));
                    penalty += ShortfallPenaltyPerStat * Mathf.Clamp01(shortfall);
                }
            }
            void AddMaxPenalty(WeaponStat stat, float value, float max)
            {
                if (value - 1e-4f > max)
                {
                    var excess = (value - max) / Mathf.Max(1e-3f, GetRange(stat));
                    penalty += ShortfallPenaltyPerStat * Mathf.Clamp01(excess);
                }
            }

            AddMinPenalty(WeaponStat.Damage,     s.Damage,     req.RecommendedMinimum.Damage);
            AddMinPenalty(WeaponStat.Balance,    s.Balance,    req.RecommendedMinimum.Balance);
            AddMinPenalty(WeaponStat.Distance,   s.Distance,   req.RecommendedMinimum.Distance);
            AddMinPenalty(WeaponStat.AmmoCount,  s.AmmoCount,  req.RecommendedMinimum.AmmoCount);
            if (req.ReloadTimeIsMaxNotMin)
                 AddMaxPenalty(WeaponStat.ReloadTime, s.ReloadTime, req.RecommendedMinimum.ReloadTime);
            else AddMinPenalty(WeaponStat.ReloadTime, s.ReloadTime, req.RecommendedMinimum.ReloadTime);
            AddMinPenalty(WeaponStat.Zoom,       s.Zoom,       req.RecommendedMinimum.Zoom);

            return baseScore - penalty;

            float GetRange(WeaponStat stat)
            {
                var r = stat switch
                {
                    WeaponStat.Damage     => Damage,
                    WeaponStat.Balance    => Balance,
                    WeaponStat.Distance   => Distance,
                    WeaponStat.AmmoCount  => AmmoCount,
                    WeaponStat.ReloadTime => ReloadTime,
                    WeaponStat.Zoom       => Zoom,
                    _ => new StatRange { Min = 0, Max = 1 }
                };
                return Mathf.Max(1e-3f, r.Max - r.Min);
            }
        }
    }
}
