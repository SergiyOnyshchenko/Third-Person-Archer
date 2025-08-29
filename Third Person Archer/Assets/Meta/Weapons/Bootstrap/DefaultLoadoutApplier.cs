using System.Linq;
using UnityEngine;

namespace Meta.Weapons
{
    /// <summary>
    /// Seeds default weapons on first run (or when missing), and equips exactly one of them.
    /// </summary>
    public class DefaultLoadoutApplier
    {
        private const string AppliedFlagKey = "WeaponsDefaultsApplied_v1";

        private readonly IWeaponRepository weaponRepository;
        private readonly IEquipmentService equipmentService;
        private readonly WeaponDef[] catalog;
        private readonly DefaultLoadoutConfig config;

        public DefaultLoadoutApplier(IWeaponRepository weaponRepository,
                                     IEquipmentService equipmentService,
                                     WeaponDef[] catalog,
                                     DefaultLoadoutConfig config)
        {
            this.weaponRepository = weaponRepository;
            this.equipmentService = equipmentService;
            this.catalog = catalog;
            this.config = config;
        }

        public void ApplyIfNeeded()
        {
            if (config == null || config.DefaultWeaponIds == null || config.DefaultWeaponIds.Length == 0)
                return;

            // Use your SaveSystem (or PlayerPrefs) to gate "once".
            var applied = SaveSystem.Load(AppliedFlagKey, false);
            var state = weaponRepository.Load();

            if (applied && config.OnlyWhenNoWeaponsOwned)
            {
                // Already applied once; nothing to do if we require "fresh profile only".
                return;
            }

            bool ownsAny = state.Weapons.Any(w => w.Owned);
            if (config.OnlyWhenNoWeaponsOwned && ownsAny)
            {
                SaveSystem.Save(AppliedFlagKey, true);
                return;
            }

            // Grant missing defaults
            foreach (var id in config.DefaultWeaponIds)
            {
                var def = catalog.FirstOrDefault(d => d.Id == id);
                if (def == null) continue;

                var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == id);
                if (inst == null)
                {
                    inst = new WeaponInstance { WeaponId = id, Owned = true, Equipped = false };
                    state.Weapons.Add(inst);
                }
                else
                {
                    inst.Owned = true;
                }
            }

            weaponRepository.Save(state);

            // Equip the first valid default (global exclusive)
            var firstDefault = config.DefaultWeaponIds
                .Select(id => state.Weapons.FirstOrDefault(w => w.WeaponId == id && w.Owned))
                .FirstOrDefault(w => w != null);

            if (firstDefault != null)
            {
                equipmentService.Equip(firstDefault.WeaponId);
            }

            SaveSystem.Save(AppliedFlagKey, true);
        }
    }
}
