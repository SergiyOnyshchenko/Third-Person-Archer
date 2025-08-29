using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons.UI.Upgrade
{
    [CreateAssetMenu(menuName = "Meta/Weapons/UI/Upgrade Focus Map", fileName = "WeaponUpgradeFocusMap")]
    public class WeaponUpgradeFocusMap : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            public string PartId;            // e.g., "scope", "mag", exact WeaponPartDef.Id
            public Vector3 LocalPosition;    // where to move the model (local)
            public Vector3 LocalEulerAngles; // how to rotate to showcase the part
        }

        [SerializeField] private List<Entry> entries = new();
        private Dictionary<string, Entry> cache;

        private void OnEnable()
        {
            cache = new Dictionary<string, Entry>(StringComparer.Ordinal);
            foreach (var e in entries) if (!cache.ContainsKey(e.PartId)) cache[e.PartId] = e;
        }

        public bool TryGet(string partId, out Entry e) => cache.TryGetValue(partId, out e);
    }
}