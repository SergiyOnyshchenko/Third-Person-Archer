// Assets/Meta/Weapons/UI/Display/WeaponPartFocusLibrary.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons.UI.Display
{
    [CreateAssetMenu(menuName = "Meta/Weapons/UI/Part Focus Library (by Class & Slot)", fileName = "WeaponPartFocusLibrary")]
    public class WeaponPartFocusLibrary : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable]
        public struct Entry
        {
            public WeaponClass Class;
            public PartSlotType Slot;
            public Vector3 LocalPosition;
            public Vector3 LocalEulerAngles;
        }

        [SerializeField] private List<Entry> entries = new();

        // Safer than ValueTuple for IL2CPP: a tiny key struct
        private struct Key : IEquatable<Key>
        {
            public readonly WeaponClass C; public readonly PartSlotType S;
            public Key(WeaponClass c, PartSlotType s) { C = c; S = s; }
            public bool Equals(Key other) => C == other.C && S == other.S;
            public override bool Equals(object obj) => obj is Key k && Equals(k);
            public override int GetHashCode() => ((int)C * 397) ^ (int)S;
        }

        [NonSerialized] private Dictionary<Key, Entry> map;
        [NonSerialized] private int version;
        public int Version => version;

        public bool TryGet(WeaponClass cls, PartSlotType slot, out Entry e)
        {
            RebuildIfNeeded();
            if (map.TryGetValue(new Key(cls, slot), out e)) return true;
            e = default; return false; // ✅ fix for CS0177 you saw earlier
        }

        // -------- cache maintenance --------
        private void RebuildIfNeeded()
        {
            if (map == null || map.Count != entries.Count) RebuildCache();
        }

        private void RebuildCache()
        {
            map ??= new Dictionary<Key, Entry>();
            map.Clear();
            foreach (var e in entries) map[new Key(e.Class, e.Slot)] = e;
            version++;
        }

        private void OnEnable()    => RebuildCache();
#if UNITY_EDITOR
        private void OnValidate()  => RebuildCache();
#endif
        public void OnAfterDeserialize() => RebuildCache();
        public void OnBeforeSerialize() { }

#if UNITY_EDITOR
        [ContextMenu("Rebuild Cache Now")]
        private void ContextRebuild() => RebuildCache();
#endif
    }
}
