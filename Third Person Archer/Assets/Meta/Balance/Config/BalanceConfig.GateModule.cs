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
        public sealed class ClassZoneTiers
        {
            [Tooltip("Weapon class.")]
            public WeaponClass Class;

            [Tooltip("Weapon tier (1..4) the class is balanced for in each zone. Index = zone index (Zone 1 = 0).")]
            public int[] TierByZone = { 1, 2, 3 };
        }

        [Header("Legacy Damage Profiles (debug easy mode; fallback when a tier is missing)")]
        [Tooltip("Old formula: BaseDamage + MissionGrowth * globalIndex. Campaign, Sniper and Boss use weapon tiers instead.")]
        [SerializeField] private WeaponGateProfile[] _weaponGateProfiles;

        [Header("Tier Gates (Campaign R, Sniper, Boss)")]
        [Tooltip("Which tier each class is balanced for per zone. Gate = tier damage at the mission's position " +
                 "among same-class missions of the zone (first = tier base, then towards tier max).")]
        [SerializeField] private ClassZoneTiers[] _classZoneTiers;

        [Tooltip("Boss gate = zone's Crossbow tier damage at this position of its level range (0 = base, 1 = max).")]
        [Range(0f, 1f)] [SerializeField] private float _bossTierPosition = 0.5f;

        public float BossTierPosition => _bossTierPosition;

        [Header("Campaign Soft Gate (1–2 shot rule)")]
        [Tooltip("Tier gate value = recommended power R (enemy dies in 1 body shot at R). " +
                 "Mission is playable from R * fraction (Yellow). >= 0.5 guarantees at most 2 body shots.")]
        [Range(0.5f, 1f)] [SerializeField] private float _campaignMinDamageFraction = 0.5f;

        public float CampaignMinDamageFraction => _campaignMinDamageFraction;

        /// <summary>Tier the class is balanced for in the zone (last entry repeats). 0 = not configured.</summary>
        public int GetTierForZone(WeaponClass weaponClass, int zoneIndex)
        {
            if (_classZoneTiers == null) return 0;

            foreach (var t in _classZoneTiers)
            {
                if (t == null || t.Class != weaponClass || t.TierByZone == null || t.TierByZone.Length == 0)
                    continue;

                return t.TierByZone[Mathf.Clamp(zoneIndex, 0, t.TierByZone.Length - 1)];
            }

            return 0;
        }

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
