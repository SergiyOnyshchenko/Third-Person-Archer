// Assets/Scripts/Runtime/Weapons/Equipping/WeaponVisualResolverUsingMap.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using Meta.Weapons; 
using Meta.Weapons.UI.Selection;
using Game.Weapons;

namespace Game.Weapons
{
    /// <summary>
    /// Visual resolver that pulls Third-Person models from an IWeaponPrefabProvider
    /// (e.g., your WeaponPrefabMap), with optional per-weapon overrides and
    /// per-class defaults. FPV is driven by weaponId strings only.
    /// </summary>
    [CreateAssetMenu(menuName = "Weapons/Weapon Visual Resolver (Using Prefab Map)",
                     fileName = "WeaponVisualResolver_UsingMap")]
    public sealed class WeaponVisualResolverUsingMap : ScriptableObject, IWeaponVisualResolver, ISerializationCallbackReceiver
    {
        [Header("Prefab Provider")]
        [Tooltip("ScriptableObject implementing IWeaponPrefabProvider (e.g., WeaponPrefabMap)")]
        [SerializeField] private ScriptableObject prefabProviderAsset; // must implement IWeaponPrefabProvider

        // Optional per-weapon overrides
        [Serializable]
        public sealed class WeaponEntry
        {
            public WeaponDef Weapon;

            [Tooltip("Override Third-Person model. If null, we use Prefab Provider result.")]
            public GameObject ThirdPersonOverride;

            [Tooltip("Override FPV weaponId used by FPV skin views. Leave empty to use Weapon.Id.")]
            public string FirstPersonWeaponIdOverride;
        }

        // Optional per-class defaults
        [Serializable]
        public sealed class ClassDefault
        {
            public WeaponClass Class;
            [Tooltip("TPV default if provider and per-weapon override are missing.")]
            public GameObject ThirdPersonDefault;
            [Tooltip("FPV id default if a weapon doesn't specify one; empty = fall back to Weapon.Id.")]
            public string FirstPersonWeaponIdDefault;
        }

        [Header("Per-Weapon Overrides (optional)")]
        [SerializeField] private List<WeaponEntry> perWeapon = new();

        [Header("Per-Class Defaults (optional)")]
        [SerializeField] private List<ClassDefault> perClass = new();

        // runtime cache
        private IWeaponPrefabProvider _provider;
        private Dictionary<string, WeaponEntry> _byId;
        private Dictionary<WeaponClass, ClassDefault> _byClass;

        public IWeaponVisualResolver.Visuals Resolve(WeaponDef definition)
        {
            if (definition == null) return new IWeaponVisualResolver.Visuals(null, null);

            EnsureCaches();

            // 1) FPV id
            string fpvId = definition.Id;
            if (_byId.TryGetValue(definition.Id, out var we) && !string.IsNullOrWhiteSpace(we.FirstPersonWeaponIdOverride))
                fpvId = we.FirstPersonWeaponIdOverride;
            else if (_byClass.TryGetValue(definition.Class, out var cd1) && !string.IsNullOrWhiteSpace(cd1.FirstPersonWeaponIdDefault))
                fpvId = cd1.FirstPersonWeaponIdDefault;

            // 2) TPV model (priority: per-weapon override → provider → class default)
            GameObject tpvModel = null;

            if (we != null && we.ThirdPersonOverride != null)
                tpvModel = we.ThirdPersonOverride;
            else if (_provider != null && !string.IsNullOrEmpty(definition.Id))
                tpvModel = _provider.GetPrefab(definition.Id); // WeaponPrefabMap lookup
            if (tpvModel == null && _byClass.TryGetValue(definition.Class, out var cd2))
                tpvModel = cd2.ThirdPersonDefault;

            return new IWeaponVisualResolver.Visuals(tpvModel, fpvId);
        }

        private void EnsureCaches()
        {
            if (_provider == null)
                _provider = prefabProviderAsset as IWeaponPrefabProvider;

            if (_byId == null)
            {
                _byId = new Dictionary<string, WeaponEntry>(StringComparer.Ordinal);
                foreach (var e in perWeapon)
                {
                    if (e?.Weapon == null || string.IsNullOrWhiteSpace(e.Weapon.Id)) continue;
                    if (!_byId.ContainsKey(e.Weapon.Id)) _byId.Add(e.Weapon.Id, e);
                }
            }

            if (_byClass == null)
            {
                _byClass = new Dictionary<WeaponClass, ClassDefault>();
                foreach (var e in perClass)
                {
                    if (e == null) continue;
                    _byClass[e.Class] = e; // last wins
                }
            }
        }

        // keep caches fresh in editor/runtime reloads
        private void ClearCaches()
        {
            _provider = null;
            _byId = null;
            _byClass = null;
        }

        private void OnEnable()   => ClearCaches();
#if UNITY_EDITOR
        private void OnValidate() => ClearCaches();
#endif
        public void OnAfterDeserialize() => ClearCaches();
        public void OnBeforeSerialize() { }
    }
}