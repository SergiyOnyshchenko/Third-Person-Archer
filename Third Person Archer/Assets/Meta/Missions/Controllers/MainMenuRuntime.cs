using System;
using UnityEngine;
using Meta.Weapons;

public sealed class MainMenuRuntime : MonoBehaviour
{
    public static MainMenuRuntime Instance { get; private set; }

    /// <summary>Fires when Services are created and ready to use.</summary>
    public static event Action<MainMenuServices> Ready;

    [Header("Data (ScriptableObjects)")]
    [SerializeField] private BalanceConfig _balanceConfig;
    [SerializeField] private MetaLoopProgressData _loopProgressData;
    [SerializeField] private MetaModeUnlockConfig _modeUnlockConfig;
    [SerializeField] private MissionLaunchRequest _launchRequest;
    [SerializeField] private LoadoutSnapshot _loadoutSnapshot;

    public MainMenuServices Services { get; private set; }
    public MissionProgressData ProgressData { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("MainMenuRuntime: duplicate instance in scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (!DataManager.Instance.TryGetData(out MissionProgressData progress) || progress == null)
        {
            Debug.LogError("MainMenuRuntime: MissionProgressData not found in DataManager.");
            enabled = false;
            return;
        }

        if (_balanceConfig == null || _modeUnlockConfig == null || _launchRequest == null)
        {
            Debug.LogError("MainMenuRuntime: required configs are not assigned.");
            enabled = false;
            return;
        }

        if (_loadoutSnapshot == null)
        {
            Debug.LogError("MainMenuRuntime: LoadoutSnapshot is not assigned. Assign the same asset used by WeaponsInitializer.");
            enabled = false;
            return;
        }

        // Enforce your 5-slot invariant (safe to call multiple times)
        _loadoutSnapshot.EnsureAllClassesExist(); // :contentReference[oaicite:3]{index=3}

        ProgressData = progress;

        // Rule: only current map accessible => force zone to last unlocked
        int lastUnlocked = ProgressData.GetLastUnlockedZoneIndex();
        ProgressData.SelectZone(lastUnlocked);

        BuildServices();

        // Any selection change should notify UI to refresh
        ProgressData.OnZoneChanged.AddListener(NotifyStateChanged);
        ProgressData.OnMissionTypeChanged.AddListener(NotifyStateChanged);

        NotifyStateChanged();
        Ready?.Invoke(Services);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (ProgressData != null)
        {
            ProgressData.OnZoneChanged.RemoveListener(NotifyStateChanged);
            ProgressData.OnMissionTypeChanged.RemoveListener(NotifyStateChanged);
        }
    }

    private void BuildServices()
    {
        var progressRead = new MissionProgressReadAdapter(ProgressData);
        var progressWrite = new MissionProgressWriteAdapter(ProgressData);

        var loopProgress = new LoopProgressService(_loopProgressData, maxBalancedLoopIndex: 2);

        var catalog = new MissionCatalogService(ProgressData.AllZones);

        // Uses MissionData weapon fields (Option A)
        var weaponReq = new WeaponRequirementService(rotationStep: 1);

        var context = new MissionContextService(progressRead, catalog, loopProgress, weaponReq);

        var loadoutStats = new LoadoutSnapshotWeaponStatService(_loadoutSnapshot);

        var gate = new MissionGateService(_balanceConfig, loadoutStats);

        var companyLevel = new CompanyLevelService(progressRead);
        var availability = new MissionAvailabilityService(gate, _modeUnlockConfig, companyLevel);

        var contractPool = new ContractPoolService(ProgressData);

        var missionStart = new MissionStartService(
            ProgressData,
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
            missionStart,

            ProgressData,
            _launchRequest,
            _loopProgressData,
            catalog,
            weaponReq
        );
    }

    private void NotifyStateChanged()
    {
        Services?.NotifyMenuStateChanged();
    }
}