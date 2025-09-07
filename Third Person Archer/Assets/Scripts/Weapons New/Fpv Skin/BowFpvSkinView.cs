using System;
using System.Collections.Generic;
using UnityEngine;
using Actor;

namespace Game.Weapons
{
    public sealed class BowFpvSkinView : Actor.System, IBowView
    {
        [Serializable]
        public sealed class Entry
        {
            [Tooltip("WeaponDef.Id or any unique string key for this variant")]
            public string WeaponId;

            [Tooltip("The BowView prefab/child that contains Model/BowSpring/Arrow for this variant")]
            public BowView View;
        }

        [Header("Catalog")]
        [SerializeField] private List<Entry> entries = new();

        [Header("Optional default shown at Awake (leave empty to show nothing)")]
        [SerializeField] private string defaultWeaponId;

        private readonly Dictionary<string, BowView> _map = new(StringComparer.Ordinal);
        private BowView _current;

        #region IBowView (forward to current)
        public GameObject Model => _current ? _current.Model : null;
        public BowSpring BowSpring => _current ? _current.BowSpring : null;
        public GameObject Arrow => _current ? _current.Arrow : null;
        #endregion

        private void Awake()
        {
            RebuildMap();
            HideAll();
            if (!string.IsNullOrEmpty(defaultWeaponId))
            {
                SetView(defaultWeaponId);
            }
        }

        private void OnValidate()
        {
            // keep keys unique & non-empty in editor, disable duplicates
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var e in entries)
            {
                if (e == null) continue;
                if (string.IsNullOrWhiteSpace(e.WeaponId)) continue;
                if (!seen.Add(e.WeaponId))
                {
                    Debug.LogWarning($"BowFpvSkinView: duplicate WeaponId '{e.WeaponId}' on {name}. Only the first will be used.");
                }
            }
        }

        /// <summary>Switch active FPV set by weapon id. Returns true on success.</summary>
        public bool SetView(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
            {
                Debug.LogWarning("BowFpvSkinView.SetView called with null/empty id.");
                return false;
            }

            if (_map.Count == 0) RebuildMap();

            if (!_map.TryGetValue(weaponId, out var view) || view == null)
            {
                Debug.LogWarning($"BowFpvSkinView: no entry for id '{weaponId}' on {name}.");
                return false;
            }

            if (_current == view) return true;

            HideAll();
            _current = view;
            _current.Model.SetActive(true);
            return true;
        }

        private void RebuildMap()
        {
            _map.Clear();
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.WeaponId) || e.View == null) continue;
                if (_map.ContainsKey(e.WeaponId)) continue; // first wins
                _map.Add(e.WeaponId, e.View);
            }
        }

        private void HideAll()
        {
            foreach (var e in entries)
            {
                if (e?.View?.Model != null) e.View.Model.SetActive(false);
            }
            _current = null;
        }
    }
}