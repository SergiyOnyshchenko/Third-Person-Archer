// Optional: attach to a bootstrap scene object to wire services in your IoC container.
using UnityEngine;
using Meta.Economy;
using System.Linq;

namespace Meta.Weapons
{
    public class WeaponsInitializer : MonoBehaviour
    {
        [Header("Catalog")]
        [SerializeField] private WeaponCatalog _weaponCatalog;
        [SerializeField] private DefaultLoadoutConfig _defaultLoadoutConfig;
 
        // Example of how you might construct services, then expose them to your game:
        public IWeaponRepository WeaponRepository { get; private set; }
        public IUpgradeJobsRepository JobsRepository { get; private set; }
        public IStatsService StatsService { get; private set; }
        public IEquipmentService EquipmentService { get; private set; }
        public IUpgradeService UpgradeService { get; private set; }
        public IMissionGateService MissionGateService { get; private set; }

        // Provide these from your composition root / DI in a real project
        private IWalletService _wallet;
        private ITimeProvider _time;

        private void Awake()
        {
            WeaponRepository = new WeaponRepository();
            JobsRepository = new UpgradeJobsRepository();
            StatsService = new StatsService();

            // Plug in your wallet implementation & time provider
            _wallet = new YourWalletService();         // TODO: replace with your real service
            _time = new SystemTimeProvider();

            EquipmentService = new EquipmentService(WeaponRepository);
            UpgradeService = new UpgradeService(WeaponRepository, JobsRepository, _wallet, _time, _weaponCatalog.All.ToArray());
            MissionGateService = new MissionGateService(WeaponRepository, StatsService, _weaponCatalog.All.ToArray());

            var applier = new DefaultLoadoutApplier(WeaponRepository, EquipmentService, _weaponCatalog.All.ToArray(), _defaultLoadoutConfig);
            applier.ApplyIfNeeded();

            // Optional: finalize any due upgrades immediately on load
            UpgradeService.ProcessDueUpgrades();
        }
    }

    // Example wallet stub – replace with your real implementation
    internal class YourWalletService : IWalletService
    {
        public bool CanAfford(CurrencyType currency, int amount) => true; // TODO
        public void Spend(CurrencyType currency, int amount) { /* TODO */ }
        public int GetBalance(CurrencyType currency) => 0; // TODO
    }
}

