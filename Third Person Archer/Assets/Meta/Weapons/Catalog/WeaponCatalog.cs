using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Meta.Weapons
{
    [CreateAssetMenu(menuName = "Meta/Weapons/Weapon Catalog", fileName = "WeaponCatalog")]
    public sealed class WeaponCatalog : ScriptableObject, ISerializationCallbackReceiver
    {
        [Tooltip("All weapon definitions in the game. Fill manually or click 'Refresh From Project' in the context menu (Editor only).")]
        [SerializeField] private List<WeaponDef> items = new();

        [Header("Editor Auto-Sync (optional)")]
        [Tooltip("If true, the catalog can be refreshed from the project with one click (Editor-only).")]
        [SerializeField] private bool enableEditorAutoRefresh = true;

        [Tooltip("Folders to search for WeaponDef assets when refreshing. Leave empty to scan the whole project.")]
        [SerializeField] private string[] editorSearchFolders;

        // --------- Runtime caches ---------
        [NonSerialized] private Dictionary<string, WeaponDef> _byId;
        [NonSerialized] private Dictionary<WeaponClass, List<WeaponDef>> _byClass;
        [NonSerialized] private int _version;
        public int Version => _version;

        public IReadOnlyList<WeaponDef> All => items;

        public bool TryGetById(string id, out WeaponDef def)
        {
            BuildCachesIfNeeded();
            return _byId.TryGetValue(id, out def);
        }

        public WeaponDef GetByIdOrNull(string id)
        {
            TryGetById(id, out var d);
            return d;
        }

        public IReadOnlyList<WeaponDef> GetByClass(WeaponClass cls)
        {
            BuildCachesIfNeeded();
            return _byClass.TryGetValue(cls, out var list) ? (IReadOnlyList<WeaponDef>)list : Array.Empty<WeaponDef>();
        }

        public bool Contains(string id)
        {
            BuildCachesIfNeeded();
            return _byId.ContainsKey(id);
        }

        public void OnBeforeSerialize() { /* no-op */ }
        public void OnAfterDeserialize() => RebuildCaches();

        private void OnEnable() => RebuildCaches();

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Keep list clean and sorted in editor for sanity.
            items = items.Where(x => x != null).Distinct().ToList();
            items.Sort((a, b) =>
            {
                int c = a.Class.CompareTo(b.Class);
                return c != 0 ? c : string.Compare(a.DisplayName, b.DisplayName, StringComparison.Ordinal);
            });
            RebuildCaches();
        }
#endif

        private void BuildCachesIfNeeded()
        {
            if (_byId == null || _byClass == null || _byId.Count != items.Count) RebuildCaches();
        }

        private void RebuildCaches()
        {
            _byId = new Dictionary<string, WeaponDef>(StringComparer.Ordinal);
            _byClass = new Dictionary<WeaponClass, List<WeaponDef>>();

            foreach (var def in items)
            {
                if (def == null) continue;

                // Unique ID enforcement
                if (string.IsNullOrEmpty(def.Id))
                {
                    Debug.LogWarning($"WeaponCatalog: WeaponDef '{def.name}' has empty Id.");
                    continue;
                }
                if (_byId.ContainsKey(def.Id))
                {
                    Debug.LogError($"WeaponCatalog: Duplicate WeaponDef Id '{def.Id}' found on '{def.name}'. Skipping.");
                    continue;
                }
                _byId.Add(def.Id, def);

                if (!_byClass.TryGetValue(def.Class, out var list))
                {
                    list = new List<WeaponDef>();
                    _byClass.Add(def.Class, list);
                }
                list.Add(def);
            }

            _version++;
        }

        // ------------------- EDITOR HELPERS -------------------
#if UNITY_EDITOR
        /// <summary>Editor-only: Scans the project for all WeaponDef assets and populates the list.</summary>
        public void Editor_RefreshFromProject()
        {
            if (!enableEditorAutoRefresh)
            {
                Debug.LogWarning("WeaponCatalog: Auto-refresh disabled on this asset.");
                return;
            }

            var guids = (editorSearchFolders != null && editorSearchFolders.Length > 0)
                ? UnityEditor.AssetDatabase.FindAssets("t:WeaponDef", editorSearchFolders)
                : UnityEditor.AssetDatabase.FindAssets("t:WeaponDef");

            var found = new List<WeaponDef>();
            foreach (var guid in guids)
            {
                var path = UnityEditor.AssetDatabase.GUIDToAssetPath(guid);
                var def = UnityEditor.AssetDatabase.LoadAssetAtPath<WeaponDef>(path);
                if (def != null) found.Add(def);
            }

            items = found
                .Where(x => x != null)
                .Distinct()
                .OrderBy(x => x.Class)
                .ThenBy(x => x.DisplayName)
                .ToList();

            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
            RebuildCaches();

            Debug.Log($"WeaponCatalog: refreshed with {items.Count} WeaponDef assets.");
        }

        [ContextMenu("Refresh From Project (Editor)")]
        private void Ctx_RefreshFromProject() => Editor_RefreshFromProject();
#endif
    }
}