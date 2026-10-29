using System;
using UnityEngine;
using Meta.Weapons;

public partial class BalanceConfig
{
    /// <summary>
    /// Heroic mode (after the full campaign): the same Campaign levels with R from a higher tier.
    /// Replaces the old campaign loops.
    /// </summary>
    [Serializable]
    public sealed class HeroicModule
    {
        [Serializable]
        public sealed class ClassTier
        {
            public WeaponClass Class;
            [Range(1, 4)] public int Tier = 4;
        }

        [Tooltip("Tier each class is balanced for in Heroic. R = tier damage at the level's position among " +
                 "same-class levels of the WHOLE campaign (Zone 1 Heroic = tier base, Zone 3 = tier max).")]
        [SerializeField] private ClassTier[] _tiers =
        {
            new ClassTier { Class = WeaponClass.Bow, Tier = 4 },
            new ClassTier { Class = WeaponClass.Crossbow, Tier = 4 },
            new ClassTier { Class = WeaponClass.Shuriken, Tier = 4 },
            new ClassTier { Class = WeaponClass.Spear, Tier = 3 },
            new ClassTier { Class = WeaponClass.Boomerang, Tier = 3 },
        };

        [Tooltip("Heroic Campaign reward index = global campaign index + this (rewards continue the campaign curve).")]
        [Min(0)] [SerializeField] private int _rewardIndexOffset = 23;

        [Tooltip("Guild Board after the campaign: contract reward profile index = level zone + this " +
                 "(contract enemies use the Heroic tier too).")]
        [Min(0)] [SerializeField] private int _contractZoneOffset = 3;

        [Header("Campaign finale (last level of the last zone)")]
        [Tooltip("First clear of the campaign's last level also pays the Boss reward of that zone (gold + essence of every class).")]
        [SerializeField] private bool _finaleBossReward = true;

        [Header("Replays (all zones are open in Heroic)")]
        [Tooltip("Reward of a Boss that was already beaten: cash multiplier.")]
        [Range(0f, 1f)] [SerializeField] private float _bossReplayCashMultiplier = 0.5f;
        [Tooltip("Reward of a Boss that was already beaten: essence multiplier (the tier key is paid once).")]
        [Range(0f, 1f)] [SerializeField] private float _bossReplayTokenMultiplier = 0.3f;

        public float BossReplayCashMultiplier => _bossReplayCashMultiplier;
        public float BossReplayTokenMultiplier => _bossReplayTokenMultiplier;

        public int RewardIndexOffset => _rewardIndexOffset;
        public int ContractZoneOffset => _contractZoneOffset;
        public bool FinaleBossReward => _finaleBossReward;

        public int GetTier(WeaponClass weaponClass)
        {
            if (_tiers != null)
                foreach (var t in _tiers)
                    if (t != null && t.Class == weaponClass)
                        return t.Tier;

            return 4;
        }
    }
}
