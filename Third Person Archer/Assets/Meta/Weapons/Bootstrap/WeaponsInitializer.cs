using UnityEngine;
using Meta.Economy;
using System.Linq;
using UI.Core;

namespace Meta.Weapons
{
    public class WeaponsInitializer : MonoBehaviour
    {
        public static WeaponsInitializer Instance { get; private set; }

        [SerializeField] private MissionProgressData _missionProgressData;
        [Header("Catalog")]
        [SerializeField] private WeaponCatalog _weaponCatalog;
        [SerializeField] private LoadoutSnapshot _loadoutSnapshot;
        [SerializeField] private DefaultLoadoutConfig _defaultLoadoutConfig;

        // Exposed services
        public IWeaponRepository WeaponRepository { get; private set; }
        public IStatsService StatsService { get; private set; }
        public IEquipmentService EquipmentService { get; private set; }
        public IUpgradeService UpgradeService { get; private set; }
        public IWeaponClassUnlockService ClassUnlockService { get; private set; }
        public WeaponCatalog WeaponCatalog { get => _weaponCatalog; }

        private IWallet _wallet;
        private LoadoutSnapshotUpdater _loadoutSnapshotUpdater;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            //DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        private void InitializeServices()
        {
            // Core repositories & services
            WeaponRepository = new WeaponRepository();
            StatsService = new StatsService();

            // Global wallet from your Economy
            _wallet = Meta.Economy.Economy.Wallet;

            var allDefs = _weaponCatalog.All.ToArray();

            EquipmentService = new EquipmentService(WeaponRepository);
            UpgradeService = new UpgradeService(WeaponRepository, _wallet, allDefs);

            ClassUnlockService = new WeaponClassUnlockService(_missionProgressData);
            ServiceLocator.Register<IWeaponClassUnlockService>(ClassUnlockService);
            
            // Apply default loadout once
            var applier = new DefaultLoadoutApplier(WeaponRepository, EquipmentService, allDefs, _defaultLoadoutConfig);
            applier.ApplyIfNeeded();

            if (_loadoutSnapshot != null)
            {
                _loadoutSnapshotUpdater = new LoadoutSnapshotUpdater(
                    _loadoutSnapshot,
                    WeaponRepository,
                    EquipmentService,
                    StatsService,
                    _weaponCatalog);

                _loadoutSnapshotUpdater.Initialize();

                // Subscribe to upgrade event so damage updates live:
                UpgradeService.OnWeaponUpgraded += HandleWeaponUpgradedForSnapshot;

                // Ensure a clean initial state:
                _loadoutSnapshotUpdater.RefreshAll();
            }
        }

        private void HandleWeaponUpgradedForSnapshot(string weaponId, int newLevel)
        {
            _loadoutSnapshotUpdater?.RefreshWeapon(weaponId);
        }

        private void OnDestroy()
        {
            if (UpgradeService != null)
                UpgradeService.OnWeaponUpgraded -= HandleWeaponUpgradedForSnapshot;

            _loadoutSnapshotUpdater?.Dispose();
            ServiceLocator.Unregister<IWeaponClassUnlockService>();
        }
    }
}