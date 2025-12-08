using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons.UI
{
    [CreateAssetMenu(
        menuName = "Meta/Weapons/Weapon Class Icon Library",
        fileName = "WeaponClassIconLibrary")]
    public class WeaponClassIconLibrary : ScriptableObject
    {
        [Serializable]
        public class Entry
        {
            public WeaponClass WeaponClass;
            public Sprite Icon;
        }

        [SerializeField] private Entry[] _entries;

        private Dictionary<WeaponClass, Sprite> _cache;

        private void OnEnable()
        {
            BuildCache();
        }

        private void BuildCache()
        {
            _cache = new Dictionary<WeaponClass, Sprite>();
            if (_entries == null) return;

            foreach (var entry in _entries)
            {
                if (entry == null) continue;
                _cache[entry.WeaponClass] = entry.Icon;
            }
        }

        private void EnsureCache()
        {
            if (_cache == null)
                BuildCache();
        }

        public bool TryGet(WeaponClass weaponClass, out Sprite icon)
        {
            EnsureCache();
            if (_cache != null && _cache.TryGetValue(weaponClass, out icon))
                return icon != null;

            icon = null;
            return false;
        }
    }
}