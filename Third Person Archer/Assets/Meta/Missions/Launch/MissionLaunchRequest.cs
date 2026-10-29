using Meta.Weapons;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionLaunchRequest", menuName = "GameMeta/MissionLaunchRequest", order = 10)]
public sealed class MissionLaunchRequest : ScriptableObject
{
    [SerializeField] private bool _hasRequest;

    [Header("Core")]
    [SerializeField] private MissionType _mode;
    [SerializeField] private MissionData _missionToLoad;
    [SerializeField] private WeaponClass _requiredWeaponClass;

    [SerializeField] private int _zoneIndex;
    [SerializeField] private int _globalCampaignIndex;

    [Header("Debug")]
    [SerializeField] private bool _isDebugRun;
    [SerializeField] private bool _easyDebugMode;

    [Header("Contracts (Guild Board)")]
    [SerializeField] private int _contractSlot = -1; // -1 = not a board contract
    [SerializeField] private bool _contractOrderMatched;
    [SerializeField] private bool _contractDailyBonus;
    [SerializeField] private float _contractRewardMultiplier = 1f;
    [SerializeField] private int _contractPlayerDamage;

    public bool HasRequest => _hasRequest;

    public MissionType Mode => _mode;
    public MissionData MissionToLoad => _missionToLoad;
    public WeaponClass RequiredWeaponClass => _requiredWeaponClass;

    public int ZoneIndex => _zoneIndex;
    public int GlobalCampaignIndex => _globalCampaignIndex;

    public bool IsDebugRun => _isDebugRun;
    public bool EasyDebugMode => _easyDebugMode;

    public ContractRunInfo Contract => _contractSlot >= 0
        ? new ContractRunInfo(_contractSlot, _contractOrderMatched, _contractDailyBonus, _contractRewardMultiplier, _contractPlayerDamage)
        : null;

    public void Clear()
    {
        _hasRequest = false;
        _mode = default;
        _missionToLoad = null;
        _requiredWeaponClass = default;
        _zoneIndex = 0;
        _globalCampaignIndex = -1;

        _isDebugRun = false;
        _easyDebugMode = false;

        ClearContract();
    }

    // Existing call sites keep working.
    public void Set(
        MissionType mode,
        MissionData missionToLoad,
        WeaponClass requiredWeaponClass,
        int zoneIndex,
        int globalCampaignIndex)
    {
        Set(mode, missionToLoad, requiredWeaponClass, zoneIndex, globalCampaignIndex,
            isDebugRun: false,
            easyDebugMode: false);
    }

    // New overload for debug window.
    public void Set(
        MissionType mode,
        MissionData missionToLoad,
        WeaponClass requiredWeaponClass,
        int zoneIndex,
        int globalCampaignIndex,
        bool isDebugRun,
        bool easyDebugMode)
    {
        _hasRequest = true;

        _mode = mode;
        _missionToLoad = missionToLoad;
        _requiredWeaponClass = requiredWeaponClass;

        _zoneIndex = zoneIndex;
        _globalCampaignIndex = globalCampaignIndex;

        _isDebugRun = isDebugRun;
        _easyDebugMode = easyDebugMode;

        ClearContract();
    }

    /// <summary>Call after Set(...) for a Guild Board contract.</summary>
    public void SetContract(ContractRunInfo contract)
    {
        if (contract == null)
        {
            ClearContract();
            return;
        }

        _contractSlot = contract.Slot;
        _contractOrderMatched = contract.OrderMatched;
        _contractDailyBonus = contract.DailyBonus;
        _contractRewardMultiplier = contract.RewardMultiplier;
        _contractPlayerDamage = contract.PlayerDamage;
    }

    private void ClearContract()
    {
        _contractSlot = -1;
        _contractOrderMatched = false;
        _contractDailyBonus = false;
        _contractRewardMultiplier = 1f;
        _contractPlayerDamage = 0;
    }
}