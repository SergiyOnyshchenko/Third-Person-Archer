using System;
using UnityEngine;
using Meta.Weapons;

public partial class BalanceConfig
{
    [Serializable]
    public sealed class WeaponClassPerformanceModule
    {
        [Serializable]
        public sealed class ClassDamageMultiplier
        {
            [Tooltip("Weapon class this multiplier applies to.")]
            public WeaponClass Class;

            [Tooltip("Damage multiplier for this weapon class (used by editor generators and optional tuning).")]
            [Min(0.01f)] public float DamageMultiplier = 1f;
        }

        [Header("Per Weapon Class Damage Multiplier")]
        [Tooltip("Used by generators to normalize damage differences between weapon classes.")]
        [SerializeField] private ClassDamageMultiplier[] _multipliers;

        public float GetDamageMultiplier(WeaponClass wc)
        {
            if (_multipliers == null) return 1f;

            foreach (var m in _multipliers)
            {
                if (m != null && m.Class == wc)
                    return Mathf.Max(0.01f, m.DamageMultiplier);
            }

            return 1f;
        }
    }
}