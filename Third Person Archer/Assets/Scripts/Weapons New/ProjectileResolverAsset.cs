// Assets/Scripts/Runtime/Weapons/Equipping/ProjectileResolverAsset.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Meta.Weapons;

namespace Game.Weapons
{
    [CreateAssetMenu(menuName = "Weapons/Projectile Resolver", fileName = "ProjectileResolver")]
    public sealed class ProjectileResolverAsset : ScriptableObject, IProjectileResolver, ISerializationCallbackReceiver
    {
        [Serializable]
        public sealed class WeaponOverride
        {
            [Tooltip("Specific weapon definition to override with a projectile")]
            public WeaponDef Weapon;

            [Tooltip("Projectile prefab to use for this weapon")]
            public Projectile Projectile;
        }

        [Serializable]
        public sealed class ClassDefault
        {
            [Tooltip("Default projectile for this weapon class")]
            public WeaponClass Class;

            [Tooltip("Projectile prefab to use for this class when there is no per-weapon override")]
            public Projectile Projectile;
        }

        [Header("Per-Weapon Overrides")]
        [SerializeField] private List<WeaponOverride> perWeapon = new();

        [Header("Per-Class Defaults")]
        [SerializeField] private List<ClassDefault> perClass = new();

        [Header("Global Fallback (optional)")]
        [Tooltip("Used if neither a per-weapon nor a per-class mapping exists")]
        [SerializeField] private Projectile globalFallback;

        // ---- runtime caches ----
        [NonSerialized] private Dictionary<string, Projectile> _weaponMap;
        [NonSerialized] private Dictionary<WeaponClass, Projectile> _classMap;

        // ------------- IProjectileResolver -------------
        public Projectile ResolveProjectile(WeaponDef definition)
        {
            if (definition == null) return globalFallback;
            EnsureCaches();

            // Per-weapon override
            if (!string.IsNullOrEmpty(definition.Id) &&
                _weaponMap.TryGetValue(definition.Id, out var proj) &&
                proj != null)
                return proj;

            // Per-class default
            if (_classMap.TryGetValue(definition.Class, out var classProj) && classProj != null)
                return classProj;

            // Global fallback (can be null - your shooter should handle that gracefully)
            return globalFallback;
        }

        // ------------- Cache maintenance -------------
        private void EnsureCaches()
        {
            if (_weaponMap == null || _classMap == null ||
                _weaponMap.Count != CountValidWeapons(perWeapon) ||
                _classMap.Count != CountValidClasses(perClass))
            {
                RebuildCaches();
            }
        }

        private int CountValidWeapons(List<WeaponOverride> list)
        {
            int n = 0;
            foreach (var e in list)
                if (e != null && e.Weapon != null) n++;
            return n;
        }

        private int CountValidClasses(List<ClassDefault> list)
        {
            int n = 0;
            foreach (var e in list)
                if (e != null) n++;
            return n;
        }

        private void RebuildCaches()
        {
            _weaponMap = new Dictionary<string, Projectile>(StringComparer.Ordinal);
            _classMap  = new Dictionary<WeaponClass, Projectile>();

            // Weapon overrides (first entry wins; duplicates are warned)
            var seenIds = new HashSet<string>(StringComparer.Ordinal);
            foreach (var e in perWeapon)
            {
                if (e == null || e.Weapon == null) continue;
                var id = e.Weapon.Id;
                if (string.IsNullOrWhiteSpace(id)) continue;

                if (!seenIds.Add(id))
                {
                    Debug.LogWarning($"ProjectileResolverAsset: Duplicate per-weapon entry for id '{id}'. Only the first is used.");
                    continue;
                }

                if (!_weaponMap.ContainsKey(id))
                    _weaponMap.Add(id, e.Projectile);
            }

            // Class defaults (last one wins to make edits easy)
            foreach (var e in perClass)
            {
                if (e == null) continue;
                _classMap[e.Class] = e.Projectile;
            }
        }

        // ------------- Unity lifecycle -------------
        private void OnEnable()   => RebuildCaches();
#if UNITY_EDITOR
        private void OnValidate() => RebuildCaches(); // live refresh in editor when you edit fields
#endif
        public void OnAfterDeserialize() => RebuildCaches();
        public void OnBeforeSerialize() { }

#if UNITY_EDITOR
        [ContextMenu("Validate & Rebuild Now")]
        private void ContextRefresh() => RebuildCaches();
#endif
    }
}