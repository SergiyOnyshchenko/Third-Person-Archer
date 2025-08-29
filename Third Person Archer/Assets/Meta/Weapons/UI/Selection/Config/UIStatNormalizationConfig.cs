// Assets/Scripts/Meta/Weapons/UI/Selection/UIStatNormalizationConfig.cs
using System;
using UnityEngine;

namespace Meta.Weapons.UI.Selection
{
    [CreateAssetMenu(menuName = "Meta/Weapons/UI/Stat Normalization", fileName = "UIStatNormalization")]
    public class UIStatNormalizationConfig : ScriptableObject
    {
        [Serializable] public struct StatRange { public float Min; public float Max; }
        public StatRange Damage = new(){ Min = 0,    Max = 1000 };
        public StatRange Balance = new(){ Min = 0,   Max = 100 };
        public StatRange Distance = new(){ Min = 0,  Max = 500 };
        public StatRange AmmoCount = new(){ Min = 0, Max = 30 };
        public StatRange ReloadTime = new(){ Min = 0.5f, Max = 5f }; // lower is better
        public StatRange Zoom = new(){ Min = 1, Max = 20 };

        public float Normalize(WeaponStat stat, float value)
        {
            StatRange r = stat switch
            {
                WeaponStat.Damage     => Damage,
                WeaponStat.Balance    => Balance,
                WeaponStat.Distance   => Distance,
                WeaponStat.AmmoCount  => AmmoCount,
                WeaponStat.ReloadTime => ReloadTime,
                WeaponStat.Zoom       => Zoom,
                _ => new StatRange{ Min = 0, Max = 1 }
            };
            if (Mathf.Abs(r.Max - r.Min) < 1e-6f) return 0f;

            float t = Mathf.InverseLerp(r.Min, r.Max, value);
            if (stat == WeaponStat.ReloadTime) t = 1f - t; // lower is better
            return Mathf.Clamp01(t);
        }
    }
}