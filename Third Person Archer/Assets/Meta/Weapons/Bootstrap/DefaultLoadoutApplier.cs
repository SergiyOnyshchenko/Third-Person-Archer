using System.Linq;
using UnityEngine;

namespace Meta.Weapons
{
    /// <summary>
    /// Seeds default weapons on first run (or when missing),
    /// and fills each class slot with a default weapon if needed.
    /// </summary>
    public class DefaultLoadoutApplier
    {
        private const string AppliedFlagKey = "WeaponsDefaultsApplied_v2";

        private readonly IWeaponRepository _weaponRepository;
        private readonly IEquipmentService _equipmentService;
        private readonly WeaponDef[] _catalog;
        private readonly DefaultLoadoutConfig _config;

        public DefaultLoadoutApplier(IWeaponRepository weaponRepository,
                                     IEquipmentService equipmentService,
                                     WeaponDef[] catalog,
                                     DefaultLoadoutConfig config)
        {
            _weaponRepository = weaponRepository;
            _equipmentService = equipmentService;
            _catalog          = catalog;
            _config           = config;
        }

        public void ApplyIfNeeded()
        {
            if (_config == null || _config.DefaultWeaponIds == null || _config.DefaultWeaponIds.Length == 0)
                return;

            var applied = SaveSystem.Load(AppliedFlagKey, false);
            var state   = _weaponRepository.Load();

            bool ownsAny = state.Weapons.Any(w => w.Owned);

            if (_config.OnlyWhenNoWeaponsOwned && applied && ownsAny)
                return;

            // 1) Ensure all default weapons are owned (upgrade level 0 by default)
            foreach (var id in _config.DefaultWeaponIds)
            {
                var def = _catalog.FirstOrDefault(d => d.Id == id);
                if (def == null) continue;

                var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == id);
                if (inst == null)
                {
                    inst = new WeaponInstance
                    {
                        WeaponId     = id,
                        Owned        = true,
                        UpgradeLevel = 0
                    };
                    state.Weapons.Add(inst);
                }
                else
                {
                    inst.Owned = true;
                }
            }

            _weaponRepository.Save(state);

            // 2) Fill each class slot if empty with first owned default of that class
            var byId = _catalog.ToDictionary(d => d.Id);

            FillSlotIfEmpty(state, WeaponClass.Bow,       byId);
            FillSlotIfEmpty(state, WeaponClass.Crossbow,  byId);
            FillSlotIfEmpty(state, WeaponClass.Spear,     byId);
            FillSlotIfEmpty(state, WeaponClass.Shuriken,  byId);
            FillSlotIfEmpty(state, WeaponClass.Boomerang, byId);

            _weaponRepository.Save(state);
            SaveSystem.Save(AppliedFlagKey, true);
        }

        private void FillSlotIfEmpty(WeaponsState state, WeaponClass cls,
                                     System.Collections.Generic.Dictionary<string, WeaponDef> byId)
        {
            var current = _equipmentService.GetEquippedWeaponId(state, cls);
            if (!string.IsNullOrEmpty(current))
                return;

            var candidate = _config.DefaultWeaponIds
                .Select(id => new { id, def = byId.TryGetValue(id, out var d) ? d : null })
                .Where(x => x.def != null && x.def.Class == cls)
                .Select(x => x.id)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(candidate))
            {
                _equipmentService.Equip(cls, candidate);
            }
        }
    }
}