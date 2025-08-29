using System;
using UnityEngine;

namespace Meta.Weapons
{
    [CreateAssetMenu(menuName = "Meta/Weapons/Mastery Def", fileName = "MasteryDef")]
    public class MasteryDef : ScriptableObject
    {
        [Serializable]
        public class Tier
        {
            [Min(1)] public int TierIndex = 1;
            [Min(0)] public int KillTagsCost = 1;
            [Tooltip("Percent-based multipliers (1.0 = no change, 1.05 = +5%)")]
            public StatModifier[] PercentModifiers; // use Mul = 1.05f etc., leave Add = 0
        }

        [SerializeField] private Tier[] _tiers = Array.Empty<Tier>();
        public Tier[] Tiers => _tiers;
        public int MaxTier => _tiers?.Length ?? 0;
    }
}