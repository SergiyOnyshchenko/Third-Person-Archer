using System;
using UnityEngine;
using Meta.Weapons;

public partial class BalanceConfig
{
    [Serializable]
    public sealed class GateModule
    {
        [Serializable]
        public sealed class WeaponGateProfile
        {
            [Tooltip("Weapon class this gate profile applies to.")]
            public WeaponClass Class;

            [Header("Gate Formula Components")]
            [Tooltip("Base required damage at global index 0, loop 0.")]
            public float BaseDamage = 10f;

            [Tooltip("How much required damage increases per global campaign index.")]
            public float MissionGrowth = 1f;

            [Tooltip("Additional fine-tuning curve applied: requiredDamage *= curve(globalIndex).")]
            public AnimationCurve AdditionalCurve = AnimationCurve.Linear(0, 1, 20, 1);

            [Header("Damage Cap")]
            [Tooltip("Hard cap for required damage. Raise this when adding loop multipliers so the cap does not interfere. 0 = no cap.")]
            public float DamageCap = 0f;
        }

        [Serializable]
        public sealed class SniperAccessProfile
        {
            [Header("Sniper Access Gate (Crossbow Damage Required)")]
            [Tooltip("Base Crossbow damage required to start the first Sniper mission (sniperIndex=0).")]
            public float BaseDamage = 15f;

            [Tooltip("Additional Crossbow damage required per cumulative Sniper mission completed.")]
            public float MissionGrowth = 3f;

            [Tooltip("Hard cap for Sniper access gate. 0 = no cap.")]
            public float DamageCap = 0f;
        }

        [Serializable]
        public sealed class BossAccessProfile
        {
            [Header("Boss Access Gate (Crossbow Damage Required)")]
            [Tooltip("Base Crossbow damage required to access Boss missions in the first zone (bossZoneIndex=0).")]
            public float BaseDamage = 28f;

            [Tooltip("Additional Crossbow damage required per boss zone index (Zone 1=0, Zone 2=1, etc.).")]
            public float ZoneGrowth = 9f;

            [Tooltip("Hard cap for Boss access gate. 0 = no cap.")]
            public float DamageCap = 200f;
        }

        [Header("Campaign Damage Gate Profiles Per Weapon Class")]
        [Tooltip("Required damage settings for each weapon class for Campaign missions.")]
        [SerializeField] private WeaponGateProfile[] _weaponGateProfiles;

        [Header("Sniper Access Gate")]
        [Tooltip("Crossbow damage required to access Sniper missions by tier. Uses sniperCompletedIndex as progression index.")]
        [SerializeField] private SniperAccessProfile _sniperAccessProfile = new();

        [Header("Boss Access Gate")]
        [Tooltip("Crossbow damage required to access Boss missions. Uses 0-based zone index (Zone 1 = 0, Zone 2 = 1).")]
        [SerializeField] private BossAccessProfile _bossAccessProfile = new();

        public float GetRequiredDamageForCampaign(WeaponClass weaponClass, int globalIndex)
        {
            var profile = GetGateProfile(weaponClass);
            if (profile == null)
                return 0f;

            float value = profile.BaseDamage;
            value += profile.MissionGrowth * Mathf.Max(0, globalIndex);

            if (profile.AdditionalCurve != null)
                value *= Mathf.Max(0.01f, profile.AdditionalCurve.Evaluate(globalIndex));

            if (profile.DamageCap > 0f)
                value = Mathf.Min(value, profile.DamageCap);

            return Mathf.Max(0f, value);
        }

        public float GetRequiredCrossbowDamageForSniper(int sniperCompletedIndex)
        {
            var p = _sniperAccessProfile;
            if (p == null)
                return 0f;

            float value = p.BaseDamage + p.MissionGrowth * Mathf.Max(0, sniperCompletedIndex);

            if (p.DamageCap > 0f)
                value = Mathf.Min(value, p.DamageCap);

            return Mathf.Max(0f, value);
        }

        public float GetRequiredCrossbowDamageForBoss(int bossZoneIndex)
        {
            var p = _bossAccessProfile;
            if (p == null)
                return 0f;

            float value = p.BaseDamage + p.ZoneGrowth * Mathf.Max(0, bossZoneIndex);

            if (p.DamageCap > 0f)
                value = Mathf.Min(value, p.DamageCap);

            return Mathf.Max(0f, value);
        }

        private WeaponGateProfile GetGateProfile(WeaponClass weaponClass)
        {
            if (_weaponGateProfiles == null) return null;

            foreach (var p in _weaponGateProfiles)
                if (p != null && p.Class == weaponClass)
                    return p;

            return null;
        }
    }
}
