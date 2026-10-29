using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons
{
    [CreateAssetMenu(menuName = "Weapons/Loadout Snapshot", fileName = "LoadoutSnapshot")]
    public class LoadoutSnapshot : ScriptableObject
    {
        [Serializable]
        public sealed class Slot
        {
            [SerializeField] private WeaponClass _class;
            [SerializeField] private WeaponDef _weapon;      // equipped model (visual, projectile)
            [SerializeField] private WeaponDef _powerWeapon; // best owned tier (damage range)
            [SerializeField] private int _upgradeLevel;      // class level

            // Keep old field
            [SerializeField] private float _damage;

            // NEW: computed stats (final values from IStatsService)
            [SerializeField] private WeaponStats _computedStats;

            public WeaponClass Class => _class;
            public WeaponDef Weapon => _weapon;
            public WeaponDef PowerWeapon => _powerWeapon != null ? _powerWeapon : _weapon;
            public int UpgradeLevel => _upgradeLevel;

            public float Damage => _damage;
            public WeaponStats Stats => _computedStats;

            public Slot(WeaponClass cls)
            {
                _class = cls;
            }

            public void Set(WeaponDef weapon, WeaponDef powerWeapon, int upgradeLevel, float damage, WeaponStats computedStats)
            {
                _weapon = weapon;
                _powerWeapon = powerWeapon;
                _upgradeLevel = upgradeLevel;
                _damage = damage;
                _computedStats = computedStats;
            }
        }

        [SerializeField] private List<Slot> _slots = new();

        public IReadOnlyList<Slot> Slots => _slots;

        public void EnsureAllClassesExist()
        {
            foreach (var cls in GetAllWeaponClasses())
            {
                if (FindSlot(cls) == null)
                    _slots.Add(new Slot(cls));
            }
        }

        public void SetSlot(WeaponClass cls, WeaponDef weapon, int upgradeLevel, float damage)
        {
            // Keep computed stats at least consistent for Damage if caller uses old API.
            SetSlot(cls, weapon, weapon, upgradeLevel, damage, new WeaponStats { Damage = damage });
        }

        public void SetSlot(WeaponClass cls, WeaponDef weapon, WeaponDef powerWeapon, int upgradeLevel, float damage, WeaponStats computedStats)
        {
            var slot = FindSlot(cls);
            if (slot == null)
            {
                slot = new Slot(cls);
                _slots.Add(slot);
            }

            slot.Set(weapon, powerWeapon, upgradeLevel, damage, computedStats);
        }

        public bool TryGet(WeaponClass cls, out Slot slot)
        {
            slot = FindSlot(cls);
            return slot != null;
        }

        private Slot FindSlot(WeaponClass cls)
        {
            if (_slots == null) return null;
            for (int i = 0; i < _slots.Count; i++)
            {
                if (_slots[i] != null && _slots[i].Class == cls)
                    return _slots[i];
            }
            return null;
        }

        private static WeaponClass[] GetAllWeaponClasses()
        {
            return new[]
            {
                WeaponClass.Bow,
                WeaponClass.Crossbow,
                WeaponClass.Spear,
                WeaponClass.Shuriken,
                WeaponClass.Boomerang
            };
        }
    }
}