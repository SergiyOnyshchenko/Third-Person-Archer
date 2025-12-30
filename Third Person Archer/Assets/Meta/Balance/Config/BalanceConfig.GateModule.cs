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
            [Tooltip("Hard cap for required damage. Use this because weapon max damage is not infinite. 0 = no cap.")]
            public float DamageCap = 0f;
        }

        [Header("Damage Gate Profiles Per Weapon Class")]
        [Tooltip("Required damage settings for each weapon class for Campaign missions.")]
        [SerializeField] private WeaponGateProfile[] _weaponGateProfiles;

        [Header("Generator Helper")]
        [Tooltip("Used by editor generators: expected player damage should usually be gateDamage * this factor (comfort).")]
        [SerializeField, Min(1f)] private float _gateComfortFactor = 1.2f;

        public float GateComfortFactor => _gateComfortFactor;

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

    // Compatibility alias if any code referenced BalanceConfig.WeaponGateProfile
    //[Serializable]
    //public sealed class WeaponGateProfile : GateModule.WeaponGateProfile { }
}