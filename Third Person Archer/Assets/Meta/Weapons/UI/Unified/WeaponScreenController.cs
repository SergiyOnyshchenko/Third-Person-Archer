using UnityEngine;
using Meta.Weapons.UI.Selection;
using Meta.Weapons.UI.Upgrade;
using Meta.Weapons.UI.Display;
using Meta.Economy;

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

        [Header("Backend")]
        [SerializeField] private WeaponsInitializer backend;
        [SerializeField] private WeaponDef[] catalog;
        [SerializeField] private ScriptableObject weaponIconProviderAsset;   // IWeaponIconProvider
        [SerializeField] private ScriptableObject weaponPrefabProviderAsset; // IWeaponPrefabProvider


        private WeaponScreenPresenter presenter;

        private void Awake() { }

        private void Start()
        {
            var iconProvider   = weaponIconProviderAsset as IWeaponIconProvider;
            var prefabProvider = weaponPrefabProviderAsset as IWeaponPrefabProvider;

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
                new YourWalletService(),
                iconProvider,
                prefabProvider,
                catalog,
                new SystemTimeProvider(),
                focusMap,
                poseLibrary,
                partFocusLibrary
            );

            presenter.Show();
        }
        private void Update() => presenter.Tick();
    }
}
