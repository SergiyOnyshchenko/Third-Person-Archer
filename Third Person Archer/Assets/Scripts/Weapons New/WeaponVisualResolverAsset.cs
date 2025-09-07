// Assets/Scripts/Runtime/Weapons/Equipping/WeaponVisualResolverAsset.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Meta.Weapons;

namespace Game.Weapons
{
    [CreateAssetMenu(menuName = "Weapons/Weapon Visual Resolver", fileName = "WeaponVisualResolver")]
    public sealed class WeaponVisualResolverAsset : ScriptableObject, IWeaponVisualResolver, ISerializationCallbackReceiver
    {
        [Serializable]
        public sealed class WeaponEntry
        {
            public WeaponDef Weapon;
            [Tooltip("Third-person model prefab shown by TPV holders")]
            public GameObject ThirdPersonModel;
            [Tooltip("FPV weaponId for skin views (leave empty to use Weapon.Id)")]
            public string FirstPersonWeaponIdOverride;
        }

        [Serializable]
        public sealed class ClassDefault
        {
            public WeaponClass Class;
            public GameObject ThirdPersonModel;
            [Tooltip("Optional FPV id default if a weapon doesn't specify one; leave empty to fallback to Weapon.Id")]
            public string FirstPersonWeaponIdDefault;
        }

        [Header("Per-Weapon")]
        [SerializeField] private List<WeaponEntry> perWeapon = new();

        [Header("Per-Class Defaults (optional)")]
        [SerializeField] private List<ClassDefault> perClass = new();

        // runtime caches
        [NonSerialized] private Dictionary<string, IWeaponVisualResolver.Visuals> _byId;
        [NonSerialized] private Dictionary<WeaponClass, ClassDefault> _class;

        public IWeaponVisualResolver.Visuals Resolve(WeaponDef definition)
        {
            if (definition == null) return new IWeaponVisualResolver.Visuals(null, null);
            EnsureCaches();

            if (!string.IsNullOrEmpty(definition.Id) && _byId.TryGetValue(definition.Id, out var visuals))
                return visuals;

            if (_class.TryGetValue(definition.Class, out var cd))
            {
                var fpvId = string.IsNullOrEmpty(cd.FirstPersonWeaponIdDefault) ? definition.Id : cd.FirstPersonWeaponIdDefault;
                return new IWeaponVisualResolver.Visuals(cd.ThirdPersonModel, fpvId);
            }

            // Fallback: no TPV model, FPV id = def.Id
            return new IWeaponVisualResolver.Visuals(null, definition.Id);
        }

        // cache building
        private void EnsureCaches()
        {
            if (_byId == null || _class == null) Rebuild();
        }

        private void Rebuild()
        {
            _byId = new Dictionary<string, IWeaponVisualResolver.Visuals>(StringComparer.Ordinal);
            _class = new Dictionary<WeaponClass, ClassDefault>();

            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var e in perWeapon)
            {
                if (e == null || e.Weapon == null || string.IsNullOrWhiteSpace(e.Weapon.Id)) continue;
                if (!seen.Add(e.Weapon.Id)) continue;

                var fpvId = string.IsNullOrEmpty(e.FirstPersonWeaponIdOverride) ? e.Weapon.Id : e.FirstPersonWeaponIdOverride;
                _byId[e.Weapon.Id] = new IWeaponVisualResolver.Visuals(e.ThirdPersonModel, fpvId);
            }

            foreach (var cd in perClass)
            {
                if (cd == null) continue;
                _class[cd.Class] = cd; // last wins
            }
        }

        private void OnEnable()   => Rebuild();
#if UNITY_EDITOR
        private void OnValidate() => Rebuild();
#endif
        public void OnAfterDeserialize() => Rebuild();
        public void OnBeforeSerialize() { }
    }
}