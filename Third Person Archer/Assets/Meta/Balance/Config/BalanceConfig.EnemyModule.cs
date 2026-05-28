using System;
using UnityEngine;
using Meta.Weapons;

public partial class BalanceConfig
{
    [Serializable]
    public sealed class EnemyModule
    {
        [Serializable]
        public sealed class EnemyTypeProfile
        {
            [Tooltip("Type of enemy this profile applies to.")]
            public EnemyArchetype Type;

            [Tooltip("HP multiplier applied to this enemy type.")]
            [Min(0.01f)] public float HpMultiplier = 1f;

            [Tooltip("Damage multiplier applied to this enemy type.")]
            [Min(0.01f)] public float DamageMultiplier = 1f;
        }

        [Header("Campaign Difficulty Curve")]
        [Tooltip("Difficulty scalar evaluated by global campaign index. X-axis = global index, Y-axis = difficulty.")]
        [SerializeField] private AnimationCurve _campaignDifficultyCurve = AnimationCurve.Linear(0, 1, 20, 5);

        [Tooltip("Enemy type multipliers for melee, ranged passive, ranged active.")]
        [SerializeField] private EnemyTypeProfile[] _enemyTypeProfiles;

        [Header("Auto-Leveling Difficulty (Contracts / Sniper)")]
        [Tooltip("Contracts difficulty progression: base + per completion.")]
        [SerializeField] private LinearProgression _contractsDifficulty = new() { Base = 1f, PerStep = 0.05f };

        [Tooltip("Sniper difficulty progression: base + per completion.")]
        [SerializeField] private LinearProgression _sniperDifficulty = new() { Base = 1.2f, PerStep = 0.05f };

        [Header("Weapon-Class Difficulty Tweak")]
        [Tooltip("Small per-class difficulty multipliers applied to Contracts/Sniper auto-scaling. Array indexed by WeaponClass enum.")]
        [SerializeField] private float[] _weaponClassDifficultyMultipliers = { 1f, 1f, 1f, 1f, 1f };

        [Header("Gate→Enemy conversion (global, formula-driven)")]
        [Tooltip("Enemy HP = GateDamage * factor * DifficultyScalar * TypeHpMultiplier")]
        [Min(0.01f)] public float CampaignHpFromGate = 8f;

        [Tooltip("Enemy Damage = GateDamage * factor * DifficultyScalar * TypeDamageMultiplier")]
        [Min(0.0001f)] public float CampaignDamageFromGate = 0.15f;

        [Min(0.01f)] public float ContractHpFromGate = 6.5f;
        [Min(0.0001f)] public float ContractDamageFromGate = 0.12f;

        [Min(0.01f)] public float SniperHpFromGate = 2.5f;
        [Min(0.0001f)] public float SniperDamageFromGate = 0.06f;

        [Header("Optional Multiplayer Multipliers")]
        [Min(0.01f)] public float MultiplayerHpMultiplier = 1f;
        [Min(0.01f)] public float MultiplayerDamageMultiplier = 1f;

        [Header("Campaign Health Clamp (shots-to-kill max per weapon class)")]
        [Tooltip("Per-weapon-class HP caps. Prevents Campaign enemies from becoming bullet sponges. Leave empty to disable.")]
        [SerializeField] private WeaponClassHealthProfile[] _weaponHealthProfiles;

        public float GetCampaignDifficultyScalar(int globalCampaignIndex)
        {
            return _campaignDifficultyCurve != null
                ? _campaignDifficultyCurve.Evaluate(globalCampaignIndex)
                : 1f;
        }

        public float GetContractsDifficultyScalar(int contractsCompleted)
            => _contractsDifficulty.Evaluate(contractsCompleted, 0);

        public float GetSniperDifficultyScalar(int sniperCompleted)
            => _sniperDifficulty.Evaluate(sniperCompleted, 0);

        public EnemyTypeProfile GetEnemyProfile(EnemyArchetype archetype)
        {
            if (_enemyTypeProfiles == null) return null;

            foreach (var p in _enemyTypeProfiles)
                if (p != null && p.Type == archetype)
                    return p;

            return null;
        }

        public float GetWeaponClassDifficultyMultiplier(WeaponClass wc)
        {
            int idx = (int)wc;
            if (_weaponClassDifficultyMultipliers == null ||
                idx < 0 ||
                idx >= _weaponClassDifficultyMultipliers.Length)
                return 1f;

            return _weaponClassDifficultyMultipliers[idx];
        }

        public int GetMaxShotsToKill(WeaponClass weaponClass, EnemyArchetype archetype)
        {
            if (_weaponHealthProfiles == null)
                return 0;

            foreach (var p in _weaponHealthProfiles)
            {
                if (p == null || p.WeaponClass != weaponClass)
                    continue;

                switch (archetype)
                {
                    case EnemyArchetype.Melee:         return p.MaxShotsVsMelee;
                    case EnemyArchetype.RangedPassive:  return p.MaxShotsVsRangedPassive;
                    case EnemyArchetype.RangedActive:   return p.MaxShotsVsRangedActive;
                    default:                            return p.MaxShotsVsMelee;
                }
            }

            return 0; // no profile → no clamp
        }

        public bool ShouldApplyHealthClamp(WeaponClass weaponClass, MissionType missionType)
        {
            if (_weaponHealthProfiles == null)
                return false;

            foreach (var p in _weaponHealthProfiles)
            {
                if (p == null || p.WeaponClass != weaponClass)
                    continue;

                if (missionType == MissionType.Campaign)  return p.ApplyToCampaign;
                if (missionType == MissionType.Contracts) return p.ApplyToContracts;
                return false;
            }

            return false;
        }

        public readonly struct EnemyStats
        {
            public readonly int MaxHp;
            public readonly int Damage;

            public EnemyStats(int maxHp, int damage)
            {
                MaxHp = maxHp;
                Damage = damage;
            }
        }

        [Serializable]
        public sealed class WeaponClassHealthProfile
        {
            [Tooltip("Weapon class this clamp profile applies to.")]
            public WeaponClass WeaponClass;

            [Tooltip("Max shots a barely-gated player should need to kill a Melee enemy. 0 = no clamp.")]
            [Min(0)] public int MaxShotsVsMelee = 6;

            [Tooltip("Max shots a barely-gated player should need to kill a RangedPassive enemy. 0 = no clamp.")]
            [Min(0)] public int MaxShotsVsRangedPassive = 4;

            [Tooltip("Max shots a barely-gated player should need to kill a RangedActive enemy. 0 = no clamp.")]
            [Min(0)] public int MaxShotsVsRangedActive = 2;

            [Tooltip("Apply this clamp to Campaign missions.")]
            public bool ApplyToCampaign = true;

            [Tooltip("Apply this clamp to Contracts missions. Usually false — Contracts uses its own auto-leveling.")]
            public bool ApplyToContracts = false;
        }
    }

    // Compatibility alias if you had code referencing BalanceConfig.EnemyTypeProfile
    //[Serializable]
    //public sealed class EnemyTypeProfile : EnemyModule.EnemyTypeProfile { }
}