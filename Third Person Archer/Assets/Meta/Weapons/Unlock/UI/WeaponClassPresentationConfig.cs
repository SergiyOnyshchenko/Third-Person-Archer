using System;
using System.Collections.Generic;
using UnityEngine;

namespace Meta.Weapons.Unlocks.UI
{
    [CreateAssetMenu(menuName = "Meta/Weapons/Unlocks/UI/Class Presentation Config",
                     fileName = "WeaponClassPresentationConfig")]
    public sealed class WeaponClassPresentationConfig : ScriptableObject
    {
        [Serializable]
        public sealed class Entry
        {
            public WeaponClass Class;
            public string DisplayName = "Unnamed";
            public Sprite Icon;
            public Color Accent = Color.white;
            public Color ProgressColor = new Color(0.2f, 0.8f, 0.2f);
        }

        [SerializeField] private List<Entry> entries = new();

        private Dictionary<WeaponClass, Entry> _map;

        private void OnEnable()
        {
            _map = new Dictionary<WeaponClass, Entry>();
            foreach (var e in entries) if (e != null) _map[e.Class] = e;
        }

        public bool TryGet(WeaponClass cls, out Entry entry)
        {
            if (_map == null) OnEnable();
            return _map.TryGetValue(cls, out entry);
        }
    }
}

