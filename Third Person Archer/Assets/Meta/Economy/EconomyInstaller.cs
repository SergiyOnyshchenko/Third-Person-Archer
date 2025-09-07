using UnityEngine;

namespace Meta.Economy
{
    /// <summary>
    /// Drop this on a bootstrap GameObject (e.g., in your first scene).
    /// Chooses storage, builds the WalletService, and exposes it via Economy.Wallet.
    /// </summary>
    public sealed class EconomyInstaller : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private CurrencyStartingBalanceConfig startingBalances;

        [Header("Storage")]
        [SerializeField] private bool useSaveSystem; // requires ECONOMY_USE_SAVESYSTEM

        [Header("Lifetime")]
        [SerializeField] private bool dontDestroyOnLoad = true;

        private void Awake()
        {
            if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

            ICurrencyStorage storage =
#if ECONOMY_USE_SAVESYSTEM
                useSaveSystem ? new CurrencyStorageSaveSystem() :
#endif
                new CurrencyStoragePlayerPrefs();

            var wallet = new WalletService(storage, startingBalances);
            Economy.Initialize(wallet);
        }
    }
}