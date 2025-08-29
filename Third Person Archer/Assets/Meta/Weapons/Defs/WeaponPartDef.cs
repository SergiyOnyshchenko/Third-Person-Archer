using System;
using UnityEngine;

namespace Meta.Weapons
{
    public enum PartSlotType
    {
        Grip,
        Limbs,
        Material,
        Quiver,
        Sign,
        Spearhead,
        String
    }

    [CreateAssetMenu(menuName = "Meta/Weapons/Weapon Part Def", fileName = "WeaponPartDef")]
    public class WeaponPartDef : ScriptableObject
    {
        [Serializable]
        public class Level
        {
            [Min(1)] public int LevelIndex = 1;
            [Min(0)] public int CashCost = 0;
            [Min(0)] public int GoldCost = 0;
            [Min(0)] public int BuildSeconds = 0; // cash path uses timer; gold path is instant
            public StatModifier[] Modifiers;
        }

        [SerializeField] private string _id = "part_id";
        [SerializeField] private string _displayName = "Part Name";
        [SerializeField] private PartSlotType _slotType;
        [SerializeField] private Level[] _levels = Array.Empty<Level>();
        [SerializeField] private MasteryDef _mastery; // optional, for post-cap Mastery

        public string Id => _id;
        public string DisplayName => _displayName;
        public PartSlotType SlotType => _slotType;
        public Level[] Levels => _levels;
        public MasteryDef Mastery => _mastery;
        public int MaxLevel => _levels?.Length ?? 0;
    }
}