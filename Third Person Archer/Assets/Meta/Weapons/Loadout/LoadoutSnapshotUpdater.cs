using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Meta.Weapons
{
    /// <summary>
    /// Keeps LoadoutSnapshot in sync with current saved loadout + upgrades.
    /// Source of truth remains WeaponsState / EquipmentService.
    /// </summary>
    public sealed class LoadoutSnapshotUpdater : IDisposable
    {
        private readonly LoadoutSnapshot _snapshot;
        private readonly IWeaponRepository _weaponRepo;
        private readonly IEquipmentService _equipmentService;
        private readonly IClassProgressionService _progression;
        private readonly WeaponCatalog _catalog;

        private Dictionary<string, WeaponDef> _defById;

        public LoadoutSnapshotUpdater(
            LoadoutSnapshot snapshot,
            IWeaponRepository weaponRepo,
            IEquipmentService equipmentService,
            IClassProgressionService progression,
            WeaponCatalog catalog)
        {
            _snapshot = snapshot;
            _weaponRepo = weaponRepo;
            _equipmentService = equipmentService;
            _progression = progression;
            _catalog = catalog;
        }

        public void Initialize()
        {
            if (_snapshot == null) return;

            _snapshot.EnsureAllClassesExist();
            RebuildDefIndex();

            // Subscribe to events (added below if missing)
            _equipmentService.OnEquippedWeaponChanged += HandleEquippedChanged;
            _progression.OnProgressChanged += HandleProgressChanged;

            // Upgrade event may exist on IUpgradeService, but we usually have it on UpgradeService instance:
            // We’ll subscribe from WeaponsInitializer where UpgradeService exists.
            // (see WeaponsInitializer changes section)

            RefreshAll();
        }

        public void Dispose()
        {
            if (_equipmentService != null)
                _equipmentService.OnEquippedWeaponChanged -= HandleEquippedChanged;
            if (_progression != null)
                _progression.OnProgressChanged -= HandleProgressChanged;
        }

        public void RefreshAll()
        {
            if (_snapshot == null) return;
            if (_weaponRepo == null || _equipmentService == null || _progression == null) return;

            var state = _weaponRepo.Load();
            if (state == null) return;

            foreach (var cls in GetAllWeaponClasses())
            {
                var equippedId = _equipmentService.GetEquippedWeaponId(state, cls);
                WriteSlotFromState(cls, equippedId, state);
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_snapshot);
#endif
        }

        public void RefreshWeapon(string weaponId)
        {
            // When a weapon is upgraded, damage might change only for its class slot (if equipped).
            // To keep it simple and safe, just refresh all.
            RefreshAll();
        }

        private void HandleProgressChanged(WeaponClass cls) => RefreshAll();

        private void HandleEquippedChanged(WeaponClass cls, string weaponId)
        {
            // Update just that slot
            var state = _weaponRepo.Load();
            WriteSlotFromState(cls, weaponId, state);

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(_snapshot);
#endif
        }

        private void WriteSlotFromState(WeaponClass cls, string equippedId, WeaponsState state)
        {
            if (_snapshot == null) return;

            // Visual = equipped model (tier or skin); power = best owned tier at the class level.
            var power = _progression.GetPowerDef(cls);
            var visual = !string.IsNullOrEmpty(equippedId) ? ResolveDef(equippedId) : null;
            if (visual == null || !_progression.IsOwned(visual.Id))
                visual = power;

            var computedStats = power != null ? _progression.GetPowerStats(cls) : WeaponStats.Zero;
            int classLevel = _progression.GetClassLevel(cls);

            _snapshot.SetSlot(cls, visual, power, classLevel, computedStats.Damage, computedStats);
        }

        private WeaponDef ResolveDef(string weaponId)
        {
            if (string.IsNullOrEmpty(weaponId)) return null;

            if (_defById == null || _defById.Count == 0)
                RebuildDefIndex();

            if (_defById != null && _defById.TryGetValue(weaponId, out var def))
                return def;

            return null;
        }

        private void RebuildDefIndex()
        {
            _defById = new Dictionary<string, WeaponDef>();
            if (_catalog == null || _catalog.All == null) return;

            foreach (var def in _catalog.All)
            {
                if (def == null) continue;
                if (string.IsNullOrEmpty(def.Id)) continue;
                _defById[def.Id] = def;
            }
        }

        private static WeaponClass[] GetAllWeaponClasses()
        {
            return new[]
            {
                WeaponClass.Bow,
                WeaponClass.Crossbow,
                WeaponClass.Spear,
                WeaponClass.Shuriken,
                WeaponClass.Boomerang
            };
        }
    }
}