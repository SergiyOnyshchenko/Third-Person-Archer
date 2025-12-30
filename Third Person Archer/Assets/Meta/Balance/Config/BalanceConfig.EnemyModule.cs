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
    }

    // Compatibility alias if you had code referencing BalanceConfig.EnemyTypeProfile
    //[Serializable]
    //public sealed class EnemyTypeProfile : EnemyModule.EnemyTypeProfile { }
}