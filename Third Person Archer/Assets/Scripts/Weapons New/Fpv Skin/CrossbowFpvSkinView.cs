using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Weapons
{
    public sealed class CrossbowFpvSkinView : Actor.System
    {
        [Serializable]
        public sealed class Entry
        {
            [Tooltip("WeaponDef.Id or any unique string key for this variant")]
            public string WeaponId;

            [Tooltip("Material to apply on FPV renderers for this weapon")]
            public Material Material;
        }

        [Header("Targets (renderers in the FPV crossbow rig)")]
        [SerializeField] private Renderer[] targetRenderers;

        [Header("Skins")]
        [SerializeField] private List<Entry> entries = new();

        [Header("Apply mode")]
        [Tooltip("If true, assigns SharedMaterial (faster, changes all instances using this prefab). If false, assigns instance Materials.")]
        [SerializeField] private bool useSharedMaterial = true;

        [Header("Optional default at Awake")]
        [SerializeField] private string defaultWeaponId;

        private readonly Dictionary<string, Material> _map = new(StringComparer.Ordinal);

        private void Awake()
        {
            RebuildMap();
            if (!string.IsNullOrEmpty(defaultWeaponId))
            {
                SetView(defaultWeaponId);
            }
        }

        private void OnValidate()
        {
            // remove nulls & duplicates in editor
            var seen = new HashSet<string>(StringComparer.Ordinal);
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var e = entries[i];
                if (e == null || string.IsNullOrWhiteSpace(e.WeaponId))
                    continue;

                if (!seen.Add(e.WeaponId))
                    Debug.LogWarning($"CrossbowFpvSkinView: duplicate WeaponId '{e.WeaponId}' on {name}. First entry wins at runtime.");
            }
        }

        /// <summary>Switch FPV crossbow material by weapon id. Returns true on success.</summary>
        public bool SetView(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId))
            {
                Debug.LogWarning("CrossbowFpvSkinView.SetView called with null/empty id.");
                return false;
            }

            if (_map.Count == 0) RebuildMap();

            if (!_map.TryGetValue(weaponId, out var mat) || mat == null)
            {
                Debug.LogWarning($"CrossbowFpvSkinView: no material mapped for id '{weaponId}' on {name}.");
                return false;
            }

            Apply(mat);
            return true;
        }

        private void Apply(Material mat)
        {
            if (targetRenderers == null) return;

            for (int i = 0; i < targetRenderers.Length; i++)
            {
                var r = targetRenderers[i];
                if (r == null) continue;

                if (useSharedMaterial)
                {
                    // Assign a single shared material (fast, but “global” per prefab)
                    r.sharedMaterial = mat;
                }
                else
                {
                    // Create or reuse per-instance materials array
                    var mats = r.materials;
                    if (mats != null && mats.Length > 0)
                    {
                        mats[0] = mat; // assuming single-material mesh in FPV; extend if needed
                        r.materials = mats;
                    }
                    else
                    {
                        r.material = mat;
                    }
                }
            }
        }

        private void RebuildMap()
        {
            _map.Clear();
            foreach (var e in entries)
            {
                if (e == null || string.IsNullOrWhiteSpace(e.WeaponId) || e.Material == null) continue;
                if (_map.ContainsKey(e.WeaponId)) continue; // first wins
                _map.Add(e.WeaponId, e.Material);
            }
        }
    }
}