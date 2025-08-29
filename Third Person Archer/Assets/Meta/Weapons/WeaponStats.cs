// Assets/Scripts/Meta/Weapons/WeaponStats.cs
using System;
using UnityEngine;

namespace Meta.Weapons
{
    [Serializable]
    public struct WeaponStats
    {
        public float Damage;
        public float Balance;
        public float Distance;
        public float AmmoCount;
        public float ReloadTime;
        public float Zoom;

        public static WeaponStats Zero => new WeaponStats();

        public static WeaponStats operator +(WeaponStats a, WeaponStats b)
        {
            return new WeaponStats
            {
                Damage = a.Damage + b.Damage,
                Balance = a.Balance + b.Balance,
                Distance = a.Distance + b.Distance,
                AmmoCount = a.AmmoCount + b.AmmoCount,
                ReloadTime = a.ReloadTime + b.ReloadTime,
                Zoom = a.Zoom + b.Zoom
            };
        }

        public WeaponStats Multiply(float factor)
        {
            return new WeaponStats
            {
                Damage = Damage * factor,
                Balance = Balance * factor,
                Distance = Distance * factor,
                AmmoCount = AmmoCount * factor,
                ReloadTime = ReloadTime * factor, // note: lower is better; mastery % buffs should handle sign appropriately
                Zoom = Zoom * factor
            };
        }

        public void ApplyAdd(WeaponStat stat, float value)
        {
            switch (stat)
            {
                case WeaponStat.Damage: Damage += value; break;
                case WeaponStat.Balance: Balance += value; break;
                case WeaponStat.Distance: Distance += value; break;
                case WeaponStat.AmmoCount: AmmoCount += value; break;
                case WeaponStat.ReloadTime: ReloadTime += value; break;
                case WeaponStat.Zoom: Zoom += value; break;
            }
        }

        public void ApplyMul(WeaponStat stat, float factor)
        {
            switch (stat)
            {
                case WeaponStat.Damage: Damage *= factor; break;
                case WeaponStat.Balance: Balance *= factor; break;
                case WeaponStat.Distance: Distance *= factor; break;
                case WeaponStat.AmmoCount: AmmoCount *= factor; break;
                case WeaponStat.ReloadTime: ReloadTime *= factor; break;
                case WeaponStat.Zoom: Zoom *= factor; break;
            }
        }
    }

    [Serializable]
    public struct StatModifier
    {
        public WeaponStat Stat;
        public float Add;   // additive delta
        public float Mul;   // multiplicative factor (1.0 = no change)

        public void Apply(ref WeaponStats stats)
        {
            if (!Mathf.Approximately(Mul, 0f) && !Mathf.Approximately(Mul, 1f))
                stats.ApplyMul(Stat, Mul);

            if (!Mathf.Approximately(Add, 0f))
                stats.ApplyAdd(Stat, Add);
        }
    }
}
