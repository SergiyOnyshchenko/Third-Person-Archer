using UnityEngine;
using Meta.Weapons;

public sealed class GameplayRuntime : MonoBehaviour
{
    public static GameplayRuntime Instance { get; private set; }

    [Header("Data (ScriptableObjects)")]
    [SerializeField] private BalanceConfig _balanceConfig;
    [SerializeField] private MissionProgressData _missionProgressData;
    [SerializeField] private MetaLoopProgressData _loopProgressData;
    [SerializeField] private MissionLaunchRequest _launchRequest;

    public MissionContext Context { get; private set; }
    public IMissionCompletionService Completion { get; private set; }
    public IMissionRewardService Rewards { get; private set; }

    public MissionProgressData ProgressData { get => _missionProgressData; }

    // Exposed for appliers/controllers
    public BalanceConfig Balance => _balanceConfig;

    // Multiplayer later. Keep stable API now.
    public bool IsMultiplayer => false;

    public int ContractsCompletedIndex => _loopProgressData != null ? _loopProgressData.ContractsCompletedIndex : 0;
    public int SniperCompletedIndex => _loopProgressData != null ? _loopProgressData.SniperCompletedIndex : 0;

    public MissionLaunchRequest LaunchRequest { get => _launchRequest; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError("GameplayRuntime: duplicate instance in scene.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (_balanceConfig == null || _launchRequest == null)
        {
            Debug.LogError("GameplayRuntime: required configs are not assigned.");
            enabled = false;
            return;
        }

        BuildServices();
        BuildContext();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void BuildServices()
    {
        var progressWrite = new MissionProgressWriteAdapter(ProgressData);
        var loopProgress = new LoopProgressService(_loopProgressData, maxBalancedLoopIndex: 2);

        Rewards = new MissionRewardService(_balanceConfig, loopProgress, _loopProgressData);
        Completion = new MissionCompletionService(progressWrite, _loopProgressData, Rewards);
    }

    private void BuildContext()
    {
        if (_launchRequest != null && _launchRequest.HasRequest)
        {
            Context = BuildContextFromLaunchRequest(_launchRequest, ProgressData);
            return;
        }

        var progressRead = new MissionProgressReadAdapter(ProgressData);

        var loopProgress = new LoopProgressService(_loopProgressData, maxBalancedLoopIndex: 2);
        var catalog = new MissionCatalogService(ProgressData.AllZones);
        var weaponReq = new WeaponRequirementService(rotationStep: 1);

        var contextService = new MissionContextService(progressRead, catalog, loopProgress, weaponReq);
        Context = contextService.BuildSelectedContext();
    }

    private static MissionContext BuildContextFromLaunchRequest(MissionLaunchRequest req, MissionProgressData progress)
    {
        var zones = progress != null ? progress.AllZones : null;
        ZoneData zone = null;

        if (zones != null && zones.Count > 0)
        {
            int zi = Mathf.Clamp(req.ZoneIndex, 0, zones.Count - 1);
            zone = zones[zi];
        }

        int globalCampaignIndex = req.GlobalCampaignIndex;
        int companyLevel = globalCampaignIndex >= 0
            ? (globalCampaignIndex + 1)
            : (progress != null ? progress.GetCompanyLevel() : -1);

        return new MissionContext(
            zone: zone,
            zoneIndex: req.ZoneIndex,
            selectedType: req.Mode,
            mission: req.MissionToLoad,
            globalCampaignIndex: globalCampaignIndex,
            companyLevel: companyLevel,
            loopIndex: req.LoopIndex,
            balanceLoopIndex: req.BalanceLoopIndex,
            requiredWeaponClass: req.RequiredWeaponClass
        );
    }

    public MissionCompleteResult CompleteMission(MissionOutcome outcome)
    {
        if (Completion == null || Context == null)
        {
            Debug.LogError("GameplayRuntime: Completion/Context not ready.");
            return new MissionCompleteResult(false, new MissionReward(0, null));
        }

        return Completion.Complete(Context, outcome);
    }
}