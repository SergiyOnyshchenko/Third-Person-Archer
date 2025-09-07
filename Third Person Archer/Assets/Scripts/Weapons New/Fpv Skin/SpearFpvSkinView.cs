// Assets/Scripts/Weapons/FPV/SpearFpvSkinView.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Weapons
{
    public sealed class SpearFpvSkinView : Actor.System
    {
        [Serializable]
        public sealed class Entry
        {
            [Tooltip("WeaponDef.Id or any unique string key for this FPV variant")]
            public string WeaponId;

            [Tooltip("Root object of this FPV spear variant (the model to show)")]
            public GameObject Root;
        }

        [Header("FPV Variants")]
        [SerializeField] private List<Entry> entries = new();

        [Header("Optional default at Awake")]
        [SerializeField] private string defaultWeaponId;

        private readonly Dictionary<string, GameObject> _map = new(StringComparer.Ordinal);
        private GameObject _current;

        private void Awake()
        {
            RebuildMap();
            HideAll();
            if (!string.IsNullOrEmpty(defaultWeaponId))
                SetView(defaultWeaponId);
        }

        private void OnValidate()
        {
            // warn on duplicate keys; first entry wins at runtime
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.WeaponId)) continue;
                if (!seen.Add(e.WeaponId))
                    Debug.LogWarning($"SpearFpvSkinView: duplicate WeaponId '{e.WeaponId}' on {name}. First entry will be used.");
            }
        }

        /// <summary>Activate the FPV model mapped to the given weaponId. Returns true on success.</summary>
        public bool SetView(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
            {
                Debug.LogWarning("SpearFpvSkinView.SetView called with null/empty id.");
                return false;
            }

            if (_map.Count == 0) RebuildMap();

            if (!_map.TryGetValue(weaponId, out var root) || root == null)
            {
                Debug.LogWarning($"SpearFpvSkinView: no entry for id '{weaponId}' on {name}.");
                return false;
            }

            if (_current == root) return true;

            HideAll();
            _current = root;
            _current.SetActive(true);
            return true;
        }

        private void RebuildMap()
        {
            _map.Clear();
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.WeaponId) || e.Root == null) continue;
                if (_map.ContainsKey(e.WeaponId)) continue; // first wins
                _map.Add(e.WeaponId, e.Root);
            }
        }

        private void HideAll()
        {
            foreach (var e in entries)
                if (e?.Root != null) e.Root.SetActive(false);
            _current = null;
        }
    }
}