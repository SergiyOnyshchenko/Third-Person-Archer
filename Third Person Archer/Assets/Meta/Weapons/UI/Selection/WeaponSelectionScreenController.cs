using UnityEngine;

namespace Meta.Weapons.UI.Selection
{
    /// <summary>
    /// Thin MonoBehaviour that assembles the presenter with your existing services and assets.
    /// </summary>
    public class WeaponSelectionScreenController : MonoBehaviour
    {
        [Header("View and Configs")]
        [SerializeField] private WeaponSelectionView selectionView;
        [SerializeField] private UIStatNormalizationConfig normalizationConfig;
        [SerializeField] private UIStyleConfig styleConfig;

        [Header("Providers")]
        [SerializeField] private ScriptableObject weaponIconProviderAsset;   // must implement IWeaponIconProvider
        [SerializeField] private ScriptableObject weaponPrefabProviderAsset; // must implement IWeaponPrefabProvider

        [Header("Backend Composition")]
        [SerializeField] private WeaponsInitializer backendInitializer; // from your core layer that exposes services
        [SerializeField] private WeaponDef[] weaponCatalog;

        private WeaponSelectionPresenter presenter;

        private void Awake()
        {
            var iconProvider   = weaponIconProviderAsset   as IWeaponIconProvider;
            var prefabProvider = weaponPrefabProviderAsset as IWeaponPrefabProvider;

            presenter = new WeaponSelectionPresenter(
                selectionView,
                normalizationConfig,
                styleConfig,
                backendInitializer.WeaponRepository,
                backendInitializer.EquipmentService,
                backendInitializer.StatsService,
                new YourWalletService(), // replace with your real wallet service instance
                iconProvider,
                prefabProvider,
                weaponCatalog
            );
        }

        private void Start()
        {
            presenter.Show();
        }
    }
}