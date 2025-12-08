using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons.UI.Selection
{

    [CreateAssetMenu(menuName = "Meta/Weapons/UI/Weapon Icon Library", fileName = "WeaponIconLibrary")]
    public class WeaponIconLibrary : ScriptableObject, IWeaponIconProvider
    {
        [Serializable]
        public struct Entry
        {
            public string WeaponId;
            public Sprite Icon;
        }

        [SerializeField] private Sprite defaultIcon;
        [SerializeField] private List<Entry> entries = new();

        private Dictionary<string, Sprite> cache;

        private void OnEnable()
        {
            cache = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (var e in entries)
            {
                if (!string.IsNullOrEmpty(e.WeaponId) && e.Icon != null)
                    cache[e.WeaponId] = e.Icon;
            }
        }

        public Sprite GetIcon(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return defaultIcon;
            if (cache != null && cache.TryGetValue(weaponId, out var sprite)) return sprite;
            return defaultIcon;
        }
    }
}