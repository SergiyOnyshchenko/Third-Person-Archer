using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons.UI.Display
{
    [CreateAssetMenu(menuName = "Meta/Weapons/UI/Display Pose Library (by Class)", fileName = "WeaponDisplayPoseLibrary")]
    public class WeaponDisplayPoseLibrary : ScriptableObject, ISerializationCallbackReceiver
    {
        [Serializable] public struct Pose { public Vector3 LocalPosition; public Vector3 LocalEulerAngles; }

        [Serializable]
        public struct Entry
        {
            public WeaponClass Class;
            public Pose SelectionPose;
            public Pose UpgradePose;
        }

        [SerializeField] private List<Entry> entries = new();

        [NonSerialized] private Dictionary<WeaponClass, Entry> map;
        [NonSerialized] private int version;

        public int Version => version;

        public bool TryGetSelectionPose(WeaponClass cls, out Pose pose)
        {
            RebuildIfNeeded();
            if (map.TryGetValue(cls, out var e)) { pose = e.SelectionPose; return true; }
            pose = default; return false;
        }

        public bool TryGetUpgradePose(WeaponClass cls, out Pose pose)
        {
            RebuildIfNeeded();
            if (map.TryGetValue(cls, out var e)) { pose = e.UpgradePose; return true; }
            pose = default; return false;
        }

        // -------- cache maintenance --------
        private void RebuildIfNeeded()
        {
            if (map == null || map.Count != entries.Count) RebuildCache();
        }

        private void RebuildCache()
        {
            map ??= new Dictionary<WeaponClass, Entry>();
            map.Clear();
            foreach (var e in entries) map[e.Class] = e; // last wins on duplicates
            version++;
        }

        // ScriptableObject lifecycle hooks
        private void OnEnable()    => RebuildCache();
#if UNITY_EDITOR
        private void OnValidate()  => RebuildCache();  // fires when you edit in inspector (play & edit mode)
#endif
        public void OnAfterDeserialize() => RebuildCache(); // covers serialization reloads
        public void OnBeforeSerialize() { }

#if UNITY_EDITOR
        [ContextMenu("Rebuild Cache Now")]
        private void ContextRebuild() => RebuildCache();
#endif
    }
}

