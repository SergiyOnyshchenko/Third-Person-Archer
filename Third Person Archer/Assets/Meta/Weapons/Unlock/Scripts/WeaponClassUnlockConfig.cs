using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Meta.Weapons;

namespace Meta.Weapons.Unlocks
{
    [CreateAssetMenu(menuName = "Meta/Weapons/Unlocks/Weapon Class Unlock Config", fileName = "WeaponClassUnlockConfig")]
    public sealed class WeaponClassUnlockConfig : ScriptableObject
    {
        [Serializable]
        public sealed class Rule
        {
            public WeaponClass Class;

            [Tooltip("If true, this class is available from the start (e.g., Bow).")]
            public bool StartsUnlocked = false;

            [Tooltip("Missions that grant credit toward unlocking this class.")]
            public List<string> MissionIds = new();

            [Tooltip("How many of the listed missions must be completed. Leave 0 to require ALL.")]
            [Min(0)] public int RequiredAnyCount = 0;

            [Tooltip("Ordering for 'next class to unlock' guidance. Lower comes earlier.")]
            public int Order = 0;

            public int RequiredCount =>
                Mathf.Max(1, RequiredAnyCount > 0 ? RequiredAnyCount : Math.Max(1, MissionIds?.Count ?? 0));
        }

        [SerializeField] private List<Rule> rules = new();

        public bool TryGetRule(WeaponClass cls, out Rule rule)
        {
            rule = rules.FirstOrDefault(r => r.Class == cls);
            return rule != null;
        }

        public IEnumerable<Rule> EnumerateOrdered() =>
            rules.OrderBy(r => r.Order).ThenBy(r => r.Class);
    }
}
