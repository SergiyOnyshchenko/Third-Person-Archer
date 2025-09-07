#nullable enable
using UnityEngine;

namespace UI.Core
{
    [CreateAssetMenu(menuName = "UI/Screen Registry", fileName = "ScreenRegistry")]
    public sealed class ScreenRegistry : ScriptableObject, IScreenRegistry
    {
        [System.Serializable]
        public struct Entry
        {
            public string Id;
            public ScreenView Prefab;
        }

        [SerializeField] private Entry[] _entries = System.Array.Empty<Entry>();

        public bool TryGetPrefab(string screenId, out ScreenView prefab)
        {
            foreach (var e in _entries)
            {
                if (e.Id == screenId) { prefab = e.Prefab; return true; }
            }
            prefab = null!;
            return false;
        }
    }
}