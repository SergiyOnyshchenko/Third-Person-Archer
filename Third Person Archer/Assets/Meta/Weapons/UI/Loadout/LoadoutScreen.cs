using System;
using System.Linq;
using UnityEngine;
using UI.Core; 

namespace Meta.Weapons.UI
{
    public class LoadoutScreen : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private WeaponCatalog _weaponCatalog;

        [Header("Slots")]
        [SerializeField] private LoadoutSlotView _bowSlot;
        [SerializeField] private LoadoutSlotView _crossbowSlot;
        [SerializeField] private LoadoutSlotView _spearSlot;
        [SerializeField] private LoadoutSlotView _shurikenSlot;
        [SerializeField] private LoadoutSlotView _boomerangSlot;

        [Header("Navigation")]
        [SerializeField] private string _weaponSelectionScreenId = "WeaponSelection"; // set in inspector
        [SerializeField] private int _debugCampaignLevel = 0; // TODO: hook to real campaign progress

        // You can keep this event if something else needs it, or remove if unused.
        public event Action<WeaponClass> OnSlotClicked;

        private IWeaponRepository _weaponRepo;
        private IStatsService _statsService;
        private IEquipmentService _equipmentService;
        private WeaponsInitializer _weaponsInitializer;

        private void Awake()
        {
            _weaponsInitializer = WeaponsInitializer.Instance;
            _weaponRepo       = _weaponsInitializer.WeaponRepository;
            _statsService     = _weaponsInitializer.StatsService;
            _equipmentService = _weaponsInitializer.EquipmentService;
        }

        private void OnEnable()
        {
            RefreshAllSlots();
        }

        public void RefreshAllSlots()
        {
            var state   = _weaponRepo.Load();
            var allDefs = _weaponCatalog.All.ToArray();

            SetupSlot(_bowSlot,       WeaponClass.Bow,       state, allDefs);
            SetupSlot(_crossbowSlot,  WeaponClass.Crossbow,  state, allDefs);
            SetupSlot(_spearSlot,     WeaponClass.Spear,     state, allDefs);
            SetupSlot(_shurikenSlot,  WeaponClass.Shuriken,  state, allDefs);
            SetupSlot(_boomerangSlot, WeaponClass.Boomerang, state, allDefs);
        }

        private void SetupSlot(LoadoutSlotView slotView, WeaponClass weaponClass, WeaponsState state, WeaponDef[] allDefs)
        {
            if (slotView == null) return;

            var equippedId = _equipmentService.GetEquippedWeaponId(state, weaponClass);
            if (string.IsNullOrEmpty(equippedId))
            {
                slotView.SetData(null, "NONE", 0f, false,
                    () => HandleSlotClick(weaponClass));
                return;
            }

            var def = allDefs.FirstOrDefault(d => d.Id == equippedId);
            if (def == null)
            {
                slotView.SetData(null, "UNKNOWN", 0f, false,
                    () => HandleSlotClick(weaponClass));
                return;
            }

            var inst = state.Weapons.FirstOrDefault(w => w.WeaponId == equippedId && w.Owned);
            var stats = _statsService.Compute(def, inst);

            // Icon is up to you – you can store it on WeaponDef or via another library.
            Sprite icon = null;

            slotView.SetData(icon, def.DisplayName, stats.Damage, true,
                () => HandleSlotClick(weaponClass));
        }

        private void HandleSlotClick(WeaponClass weaponClass)
        {
            OnSlotClicked?.Invoke(weaponClass);

            if (string.IsNullOrEmpty(_weaponSelectionScreenId))
                return;

            if (!ServiceLocator.TryResolve<IUINavigator>(out var nav))
                return;

            var args = new WeaponSelectionArgs(weaponClass, _debugCampaignLevel);
            nav.Open(_weaponSelectionScreenId, args, reuseCached: true);
        }

        // Optional helper if you want to update campaign level from outside
        public void SetCampaignLevel(int level)
        {
            _debugCampaignLevel = level;
        }
    }
}