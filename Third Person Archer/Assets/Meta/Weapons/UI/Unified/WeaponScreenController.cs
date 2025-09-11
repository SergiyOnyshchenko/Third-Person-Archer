using System;
using UnityEngine;
using Meta.Weapons.UI.Selection;
using Meta.Weapons.UI.Upgrade;
using Meta.Weapons.UI.Display;
using Meta.Economy;
using System.Linq;
using Meta.Weapons.Unlocks;

namespace Meta.Weapons.UI
{
    public class WeaponScreenController : MonoBehaviour
    {
        [Header("View")]
        [SerializeField] private WeaponScreenView view;
        [SerializeField] private SelectionBottomBarView selectionBarPrefab;
        [SerializeField] private UpgradeBottomBarView upgradeBarPrefab;
        [SerializeField] private WeaponDisplayPoseLibrary poseLibrary;
        [SerializeField] private WeaponPartFocusLibrary partFocusLibrary;

        [Header("Configs")]
        [SerializeField] private UIStatNormalizationConfig normalization;
        [SerializeField] private UIStyleConfig style;
        [SerializeField] private WeaponUpgradeFocusMap focusMap;
        [SerializeField] private WeaponClassUnlockConfig unlockConfig;
        [SerializeField] private bool useUnlocks = true;

        [Header("Backend")]
        [SerializeField] private WeaponsInitializer backend;
        [SerializeField] private WeaponCatalog catalog;
        [SerializeField] private ScriptableObject weaponIconProviderAsset;   // IWeaponIconProvider
        [SerializeField] private ScriptableObject weaponPrefabProviderAsset; // IWeaponPrefabProvider


        private WeaponScreenPresenter presenter;

        private void Awake() { }

        private void Start()
        {
            var iconProvider = weaponIconProviderAsset as IWeaponIconProvider;
            var prefabProvider = weaponPrefabProviderAsset as IWeaponPrefabProvider;

            Func<WeaponClass, bool> isUnlocked = _ => true; // default: everything visible

            if (useUnlocks && unlockConfig != null)
            {
                var repo = new WeaponClassUnlockRepository();
                var unlockService = new WeaponClassUnlockService(unlockConfig, repo);
                isUnlocked = unlockService.IsUnlocked; 
            }

            presenter = new WeaponScreenPresenter(
                view,
                selectionBarPrefab,
                upgradeBarPrefab,
                normalization,
                style,
                backend.WeaponRepository,
                backend.EquipmentService,
                backend.StatsService,
                backend.UpgradeService,
                Meta.Economy.Economy.Wallet,
                iconProvider,
                prefabProvider,
                catalog.All.ToArray(),
                new SystemTimeProvider(),
                focusMap,
                poseLibrary,
                partFocusLibrary,
                isUnlocked
            );

            presenter.Show();
        }
        private void Update() => presenter.Tick();
    }
}
