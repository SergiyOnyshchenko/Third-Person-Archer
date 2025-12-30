
using UnityEngine;

public sealed class MainMenuCompositionRoot : MonoBehaviour
{
    public static MainMenuCompositionRoot Instance { get; private set; }

    [Header("Data (ScriptableObjects)")]
    [SerializeField] private BalanceConfig _balanceConfig;
    [SerializeField] private MetaLoopProgressData _loopProgressData;
    [SerializeField] private MetaModeUnlockConfig _modeUnlockConfig;
    [SerializeField] private MissionLaunchRequest _launchRequest;

    [Header("Bridges")]
    [Tooltip("Optional. If not set, root will try to find a component in scene implementing ILoadoutWeaponStatService.")]
    [SerializeField] private MonoBehaviour _loadoutWeaponStatProvider;

    [Header("Presenters (non-map)")]
    [SerializeField] private MissionTypeSelectionPresenter _missionTypeSelectionPresenter;
    [SerializeField] private PlayMissionPresenter _playMissionPresenter;

    public MainMenuServices Services { get; private set; }

    private MissionProgressData _progressData;

    private void Awake()
    {
        Instance = this;

        if (!DataManager.Instance.TryGetData(out _progressData) || _progressData == null)
        {
            Debug.LogError("MainMenuCompositionRoot: MissionProgressData not found in DataManager.");
            enabled = false;
            return;
        }

        if (_balanceConfig == null)
        {
            Debug.LogError("MainMenuCompositionRoot: BalanceConfig is not assigned.");
            enabled = false;
            return;
        }

        if (_modeUnlockConfig == null)
        {
            Debug.LogError("MainMenuCompositionRoot: MetaModeUnlockConfig is not assigned.");
            enabled = false;
            return;
        }

        if (_launchRequest == null)
        {
            Debug.LogError("MainMenuCompositionRoot: MissionLaunchRequest is not assigned.");
            enabled = false;
            return;
        }

        // Rule: only current map accessible => force zone to last unlocked
        int lastUnlocked = _progressData.GetLastUnlockedZoneIndex();
        _progressData.SelectZone(lastUnlocked);

        // Build adapters
        var progressRead = new MissionProgressReadAdapter(_progressData);
        var progressWrite = new MissionProgressWriteAdapter(_progressData);

        // Build services
        var loopProgress = new LoopProgressService(_loopProgressData, maxBalancedLoopIndex: 2);

        var catalog = new MissionCatalogService(_progressData.AllZones);

        // Uses MissionData.BaseWeaponClass / FixedWeaponClass etc. (Option A)
        var weaponReq = new WeaponRequirementService(rotationStep: 1);

        var context = new MissionContextService(progressRead, catalog, loopProgress, weaponReq);

        var loadoutStats = ResolveLoadoutWeaponStatService();
        if (loadoutStats == null)
            Debug.LogWarning("MainMenuCompositionRoot: No ILoadoutWeaponStatService found. Campaign gates will treat damage as 0.");

        var gate = new MissionGateService(_balanceConfig, loadoutStats ?? new DummyLoadoutWeaponStatService());

        var companyLevel = new CompanyLevelService(progressRead);
        var availability = new MissionAvailabilityService(gate, _modeUnlockConfig, companyLevel);

        var contractPool = new ContractPoolService(_progressData);

        var missionStart = new MissionStartService(
            _progressData,
            context,
            availability,
            gate,
            weaponReq,
            contractPool,
            _launchRequest);

        Services = new MainMenuServices(
            progressWrite,
            context,
            gate,
            availability,
            missionStart
        );

        // Notify all presenters when selection changes
        _progressData.OnZoneChanged.AddListener(NotifyStateChanged);
        _progressData.OnMissionTypeChanged.AddListener(NotifyStateChanged);

        // Wire presenters
        if (_missionTypeSelectionPresenter != null)
            _missionTypeSelectionPresenter.Init(Services);

        if (_playMissionPresenter != null)
            _playMissionPresenter.Init(Services);

        // Initial refresh
        NotifyStateChanged();
    }

    private void OnDestroy()
    {
        if (_progressData != null)
        {
            _progressData.OnZoneChanged.RemoveListener(NotifyStateChanged);
            _progressData.OnMissionTypeChanged.RemoveListener(NotifyStateChanged);
        }

        if (Instance == this)
            Instance = null;
    }

    private void NotifyStateChanged()
    {
        Services?.NotifyMenuStateChanged();
    }

    private ILoadoutWeaponStatService ResolveLoadoutWeaponStatService()
    {
        if (_loadoutWeaponStatProvider is ILoadoutWeaponStatService direct)
            return direct;

        // Try find in children (including inactive)
        var monos = GetComponentsInChildren<MonoBehaviour>(true);
        for (int i = 0; i < monos.Length; i++)
        {
            if (monos[i] is ILoadoutWeaponStatService svc)
                return svc;
        }

        // Try find anywhere in scene
        foreach (var mb in FindObjectsOfType<MonoBehaviour>(true))
        {
            if (mb is ILoadoutWeaponStatService svc)
                return svc;
        }

        return null;
    }

    private sealed class DummyLoadoutWeaponStatService : ILoadoutWeaponStatService
    {
        public float GetEquippedDamage(Meta.Weapons.WeaponClass weaponClass) => 0f;
    }
}