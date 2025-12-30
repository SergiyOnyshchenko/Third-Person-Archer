using Meta.Weapons;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class MissionSceneBootstrapper : MonoBehaviour
{
    [Header("Cross-scene request")]
    [SerializeField] private MissionLaunchRequest _launchRequest;

    [Header("Bridges")]
    [Tooltip("Assign a component that can force gameplay to use the loadout slot for a weapon class.")]
    [SerializeField] private MonoBehaviour _loadoutApplierBehaviour;

    private IGameplayLoadoutApplier _loadoutApplier;

    private void Awake()
    {
        _loadoutApplier = _loadoutApplierBehaviour as IGameplayLoadoutApplier;
        if (_loadoutApplierBehaviour != null && _loadoutApplier == null)
            Debug.LogError("MissionSceneBootstrapper: _loadoutApplierBehaviour does not implement IGameplayLoadoutApplier.");

        if (_launchRequest == null || !_launchRequest.HasRequest)
        {
            Debug.LogWarning("MissionSceneBootstrapper: No MissionLaunchRequest. Returning to Main Menu.");
            SceneManager.LoadScene("MainMenu"); // put your real main menu scene name here
            return;
        }

        // Apply required weapon class (critical requirement)
        _loadoutApplier?.ApplyWeaponClass(_launchRequest.RequiredWeaponClass);

        // Optional: publish runtime info for gameplay systems
        RuntimeMissionSession.Set(
            _launchRequest.Mode,
            _launchRequest.MissionToLoad,
            _launchRequest.RequiredWeaponClass,
            _launchRequest.ZoneIndex,
            _launchRequest.GlobalCampaignIndex,
            _launchRequest.LoopIndex,
            _launchRequest.BalanceLoopIndex
        );

        // Important: keep the request until mission end, then clear it in mission completion pipeline.
        // (We'll do that in MissionEndPresenter later.)
    }
}

public interface IGameplayLoadoutApplier
{
    void ApplyWeaponClass(WeaponClass weaponClass);
}

public static class RuntimeMissionSession
{
    public static MissionType Mode { get; private set; }
    public static MissionData Mission { get; private set; }
    public static WeaponClass RequiredWeaponClass { get; private set; }

    public static int ZoneIndex { get; private set; }
    public static int GlobalCampaignIndex { get; private set; }
    public static int LoopIndex { get; private set; }
    public static int BalanceLoopIndex { get; private set; }

    public static void Set(
        MissionType mode,
        MissionData mission,
        WeaponClass requiredWeaponClass,
        int zoneIndex,
        int globalCampaignIndex,
        int loopIndex,
        int balanceLoopIndex)
    {
        Mode = mode;
        Mission = mission;
        RequiredWeaponClass = requiredWeaponClass;
        ZoneIndex = zoneIndex;
        GlobalCampaignIndex = globalCampaignIndex;
        LoopIndex = loopIndex;
        BalanceLoopIndex = balanceLoopIndex;
    }

    public static void Clear()
    {
        Mode = default;
        Mission = null;
        RequiredWeaponClass = default;
        ZoneIndex = 0;
        GlobalCampaignIndex = -1;
        LoopIndex = 0;
        BalanceLoopIndex = 0;
    }
}