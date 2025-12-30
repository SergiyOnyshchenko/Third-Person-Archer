using UnityEngine;

namespace Meta.Weapons
{
    [CreateAssetMenu(menuName = "Weapons/Weapon Stats Range Config", fileName = "WeaponStatsRangeConfig")]
    public sealed class WeaponStatsRangeConfig : ScriptableObject
    {
        [Header("Global UI Ranges (min / max)")]
        public WeaponStats Min = new WeaponStats
        {
            Damage = 0f,
            Balance = 0f,
            Distance = 10f,
            AmmoCount = 1f,
            ReloadTime = 0.1f,
            Zoom = 1f
        };

        public WeaponStats Max = new WeaponStats
        {
            Damage = 200f,
            Balance = 100f,
            Distance = 1000f,
            AmmoCount = 100f,
            ReloadTime = 5f,
            Zoom = 5f
        };
    }
}