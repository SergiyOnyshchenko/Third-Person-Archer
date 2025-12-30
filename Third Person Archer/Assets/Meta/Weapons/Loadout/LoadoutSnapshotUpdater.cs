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
        private readonly IStatsService _statsService;
        private readonly WeaponCatalog _catalog;

        private Dictionary<string, WeaponDef> _defById;

        public LoadoutSnapshotUpdater(
            LoadoutSnapshot snapshot,
            IWeaponRepository weaponRepo,
            IEquipmentService equipmentService,
            IStatsService statsService,
            WeaponCatalog catalog)
        {
            _snapshot = snapshot;
            _weaponRepo = weaponRepo;
            _equipmentService = equipmentService;
            _statsService = statsService;
            _catalog = catalog;
        }

        public void Initialize()
        {
            if (_snapshot == null) return;

            _snapshot.EnsureAllClassesExist();
            RebuildDefIndex();

            // Subscribe to events (added below if missing)
            _equipmentService.OnEquippedWeaponChanged += HandleEquippedChanged;

            // Upgrade event may exist on IUpgradeService, but we usually have it on UpgradeService instance:
            // We’ll subscribe from WeaponsInitializer where UpgradeService exists.
            // (see WeaponsInitializer changes section)

            RefreshAll();
        }

        public void Dispose()
        {
            if (_equipmentService != null)
                _equipmentService.OnEquippedWeaponChanged -= HandleEquippedChanged;
        }

        public void RefreshAll()
        {
            if (_snapshot == null) return;
            if (_weaponRepo == null || _equipmentService == null || _statsService == null) return;

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

            WeaponDef def = null;
            if (!string.IsNullOrEmpty(equippedId))
                def = ResolveDef(equippedId);

            int upgradeLevel = 0;
            float damage = 0f;
            var computedStats = WeaponStats.Zero;

            if (def != null && state != null)
            {
                var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id && w.Owned);
                if (inst != null)
                    upgradeLevel = inst.UpgradeLevel;

                computedStats = _statsService.Compute(def, inst);
                damage = computedStats.Damage;
            }

            _snapshot.SetSlot(cls, def, upgradeLevel, damage, computedStats);
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