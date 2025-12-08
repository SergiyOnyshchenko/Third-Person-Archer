using UnityEngine;
using Meta.Economy;
using System.Linq;

namespace Meta.Weapons
{
    public class WeaponsInitializer : MonoBehaviour
    {
        public static WeaponsInitializer Instance { get; private set; }

        [Header("Catalog")]
        [SerializeField] private WeaponCatalog _weaponCatalog;
        [SerializeField] private DefaultLoadoutConfig _defaultLoadoutConfig;

        // Exposed services
        public IWeaponRepository WeaponRepository { get; private set; }
        public IStatsService StatsService { get; private set; }
        public IEquipmentService EquipmentService { get; private set; }
        public IUpgradeService UpgradeService { get; private set; }
        public IMissionGateService MissionGateService { get; private set; }

        private IWallet _wallet;

        private void Awake()
        {
            // Singleton setup
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        private void InitializeServices()
        {
            // Core repositories & services
            WeaponRepository = new WeaponRepository();
            StatsService     = new StatsService();

            // Global wallet from your Economy
            _wallet = Meta.Economy.Economy.Wallet;

            var allDefs = _weaponCatalog.All.ToArray();

            EquipmentService  = new EquipmentService(WeaponRepository);
            UpgradeService    = new UpgradeService(WeaponRepository, _wallet, allDefs);
            MissionGateService = new MissionGateService(WeaponRepository, StatsService, allDefs);

            // Apply default loadout once
            var applier = new DefaultLoadoutApplier(WeaponRepository, EquipmentService, allDefs, _defaultLoadoutConfig);
            applier.ApplyIfNeeded();
        }
    }
}