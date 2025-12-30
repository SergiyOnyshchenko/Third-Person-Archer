using System.Linq;
using Meta.Weapons;
using UI.Core;
using UnityEngine;

namespace Meta.Weapons.UI
{
    public class LoadoutScreen : MonoBehaviour
    {
        [Header("Slots")]
        [SerializeField] private LoadoutSlotView _bowSlot;
        [SerializeField] private LoadoutSlotView _crossbowSlot;
        [SerializeField] private LoadoutSlotView _spearSlot;
        [SerializeField] private LoadoutSlotView _shurikenSlot;
        [SerializeField] private LoadoutSlotView _boomerangSlot;

        [Header("Navigation")]
        [SerializeField] private string _weaponSelectionScreenId = "WeaponSelection";

        private IWeaponRepository _weaponRepo;
        private IStatsService _statsService;
        private IEquipmentService _equipmentService;
        private IWeaponClassUnlockService _classUnlocks;
        private IUINavigator _navigator;
        private WeaponCatalog _weaponCatalog;

        private void Awake()
        {
            _weaponRepo = WeaponsInitializer.Instance.WeaponRepository;
            _statsService = WeaponsInitializer.Instance.StatsService;
            _equipmentService = WeaponsInitializer.Instance.EquipmentService;

            _classUnlocks = WeaponsInitializer.Instance.ClassUnlockService;
            _weaponCatalog = WeaponsInitializer.Instance.WeaponCatalog;

            _navigator = ServiceLocator.Resolve<IUINavigator>();
        }

        private void OnEnable()
        {
            RefreshAllSlots();
        }

        public void RefreshAllSlots()
        {
            var state = _weaponRepo.Load();
            var defs = WeaponsInitializer.Instance.WeaponCatalog.All.ToArray();

            SetupSlot(_bowSlot, WeaponClass.Bow, state, defs);
            SetupSlot(_crossbowSlot, WeaponClass.Crossbow, state, defs);
            SetupSlot(_spearSlot, WeaponClass.Spear, state, defs);
            SetupSlot(_shurikenSlot, WeaponClass.Shuriken, state, defs);
            SetupSlot(_boomerangSlot, WeaponClass.Boomerang, state, defs);
        }

        private void SetupSlot(
            LoadoutSlotView slot,
            WeaponClass weaponClass,
            WeaponsState state,
            WeaponDef[] allDefs)
        {
            if (slot == null)
                return;

            bool unlocked = _classUnlocks.IsClassUnlocked(weaponClass);
            slot.SetLocked(!unlocked);

            if (!unlocked)
            {
                slot.SetData(
                    icon: null,
                    weaponName: weaponClass.ToString(),
                    damage: 0f,
                    interactable: false,
                    onClick: null);
                return;
            }

            string equippedId = _equipmentService.GetEquippedWeaponId(state, weaponClass);
            var def = allDefs.FirstOrDefault(d => d.Id == equippedId);

            if (def == null)
            {
                slot.SetData(null, "—", 0f, false, null);
                return;
            }

            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == def.Id);
            var stats = _statsService.Compute(def, inst);

            slot.SetData(
                icon: def.Icon,
                weaponName: def.DisplayName,
                damage: stats.Damage,
                interactable: true,
                onClick: () => OpenWeaponSelection(weaponClass));
        }

        private void OpenWeaponSelection(WeaponClass weaponClass)
        {
            _navigator.Open(
                _weaponSelectionScreenId,
                new WeaponSelectionArgs(
                    weaponClass,
                    _classUnlocks.CurrentCompanyLevel),
                reuseCached: true);
        }
    }
}