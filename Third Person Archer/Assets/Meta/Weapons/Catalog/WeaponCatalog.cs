using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Meta.Weapons
{
    [CreateAssetMenu(menuName = "Meta/Weapons/Weapon Catalog", fileName = "WeaponCatalog")]
    public sealed class WeaponCatalog : ScriptableObject, ISerializationCallbackReceiver
    {
        [Tooltip("All weapon definitions in the game. Fill and order manually in the Inspector.")]
        [SerializeField] private List<WeaponDef> items = new();

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
        public void OnAfterDeserialize() => RebuildCaches(logDuplicates: false);
        private void OnEnable() => RebuildCaches(logDuplicates: false);

#if UNITY_EDITOR
        private void OnValidate()
        {
            // IMPORTANT: do not modify 'items' here (no sorting/cleaning),
            // and do not log duplicates while editing.
            RebuildCaches(logDuplicates: false);
        }
#endif

        private void BuildCachesIfNeeded()
        {
            if (_byId == null || _byClass == null)
                RebuildCaches(logDuplicates: Application.isPlaying); // only log in Play Mode
        }

        private void RebuildCaches(bool logDuplicates)
        {
            _byId = new Dictionary<string, WeaponDef>(StringComparer.Ordinal);
            _byClass = new Dictionary<WeaponClass, List<WeaponDef>>();

            for (int i = 0; i < items.Count; i++)
            {
                var def = items[i];
                if (def == null) continue;

                var id = def.Id ?? string.Empty; // keep case/spacing exactly as user set it

                if (id.Length == 0)
                {
                    if (logDuplicates)
                        Debug.LogWarning($"WeaponCatalog: A WeaponDef at index {i} ({def.name}) has empty Id.", this);
                    continue;
                }

                if (_byId.ContainsKey(id))
                {
                    if (logDuplicates)
                    {
#if UNITY_EDITOR
                        string dupPath = AssetDatabase.GetAssetPath(def);
                        string firstPath = AssetDatabase.GetAssetPath(_byId[id]);
                        Debug.LogError(
                            $"WeaponCatalog: Duplicate Id {FormatIdForDebug(id)}\n" +
                            $"  First:  index={IndexOf(items, _byId[id])}, name={_byId[id].name}, path={firstPath}\n" +
                            $"  Second: index={i}, name={def.name}, path={dupPath}",
                            this);
#else
                        Debug.LogError($"WeaponCatalog: Duplicate WeaponDef Id '{id}'.", this);
#endif
                    }
                    // Skip adding duplicate to caches
                    continue;
                }

                _byId.Add(id, def);

                if (!_byClass.TryGetValue(def.Class, out var list))
                {
                    list = new List<WeaponDef>();
                    _byClass.Add(def.Class, list);
                }

                // Preserve the order as it appears in 'items'
                list.Add(def);
            }

            _version++;
        }

#if UNITY_EDITOR
        // Right-click the asset -> "Validate Duplicates" to get a clean report
        [ContextMenu("Validate Duplicates")]
        private void ContextValidateDuplicates()
        {
            ValidateAndReportDuplicates(verbose: true);
        }
#endif

        /// <summary>Returns true if all IDs are unique; prints a minimal or verbose report.</summary>
        public bool ValidateAndReportDuplicates(bool verbose = false)
        {
            var seen = new Dictionary<string, int>(StringComparer.Ordinal);
            bool ok = true;

            for (int i = 0; i < items.Count; i++)
            {
                var def = items[i];
                if (def == null) continue;
                var id = def.Id ?? string.Empty;

                if (id.Length == 0)
                {
                    if (verbose)
                        Debug.LogWarning($"[Validate] Empty Id at index {i} ({def.name})", this);
                    ok = false;
                    continue;
                }

                if (seen.TryGetValue(id, out var firstIndex))
                {
#if UNITY_EDITOR
                    string dupPath = AssetDatabase.GetAssetPath(def);
                    string firstPath = AssetDatabase.GetAssetPath(items[firstIndex]);
                    Debug.LogError(
                        $"[Validate] Duplicate Id {FormatIdForDebug(id)}\n" +
                        $"  First:  index={firstIndex}, name={items[firstIndex].name}, path={firstPath}\n" +
                        $"  Second: index={i}, name={def.name}, path={dupPath}",
                        this);
#else
                    Debug.LogError($"[Validate] Duplicate Id '{id}' between indices {firstIndex} and {i}.", this);
#endif
                    ok = false;
                }
                else
                {
                    seen.Add(id, i);
                }
            }

            if (ok && verbose)
                Debug.Log("[Validate] All IDs are unique.", this);

            return ok;
        }

        private static int IndexOf(List<WeaponDef> list, WeaponDef target)
        {
            for (int i = 0; i < list.Count; i++)
                if (ReferenceEquals(list[i], target)) return i;
            return -1;
        }

        private static string FormatIdForDebug(string id)
        {
            if (id == null) return "null";
            var sb = new StringBuilder();
            sb.Append('"');
            foreach (var c in id)
            {
                // make zero-width / control chars visible
                if (char.IsControl(c) || c == '\u200B' || c == '\u200C' || c == '\u200D' || c == '\uFEFF')
                    sb.Append($"\\u{(int)c:x4}");
                else
                    sb.Append(c);
            }
            sb.Append('"');
            sb.Append($" (len={id.Length})");
            return sb.ToString();
        }
    }
}