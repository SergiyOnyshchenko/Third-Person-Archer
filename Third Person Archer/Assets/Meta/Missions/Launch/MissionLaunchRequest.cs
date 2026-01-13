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
    [SerializeField] private int _loopIndex;
    [SerializeField] private int _balanceLoopIndex;

    [Header("Debug")]
    [SerializeField] private bool _isDebugRun;
    [SerializeField] private bool _easyDebugMode;

    public bool HasRequest => _hasRequest;

    public MissionType Mode => _mode;
    public MissionData MissionToLoad => _missionToLoad;
    public WeaponClass RequiredWeaponClass => _requiredWeaponClass;

    public int ZoneIndex => _zoneIndex;
    public int GlobalCampaignIndex => _globalCampaignIndex;
    public int LoopIndex => _loopIndex;
    public int BalanceLoopIndex => _balanceLoopIndex;

    public bool IsDebugRun => _isDebugRun;
    public bool EasyDebugMode => _easyDebugMode;

    public void Clear()
    {
        _hasRequest = false;
        _mode = default;
        _missionToLoad = null;
        _requiredWeaponClass = default;
        _zoneIndex = 0;
        _globalCampaignIndex = -1;
        _loopIndex = 0;
        _balanceLoopIndex = 0;

        _isDebugRun = false;
        _easyDebugMode = false;
    }

    // Existing call sites keep working.
    public void Set(
        MissionType mode,
        MissionData missionToLoad,
        WeaponClass requiredWeaponClass,
        int zoneIndex,
        int globalCampaignIndex,
        int loopIndex,
        int balanceLoopIndex)
    {
        Set(mode, missionToLoad, requiredWeaponClass, zoneIndex, globalCampaignIndex, loopIndex, balanceLoopIndex,
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
        int loopIndex,
        int balanceLoopIndex,
        bool isDebugRun,
        bool easyDebugMode)
    {
        _hasRequest = true;

        _mode = mode;
        _missionToLoad = missionToLoad;
        _requiredWeaponClass = requiredWeaponClass;

        _zoneIndex = zoneIndex;
        _globalCampaignIndex = globalCampaignIndex;
        _loopIndex = loopIndex;
        _balanceLoopIndex = balanceLoopIndex;

        _isDebugRun = isDebugRun;
        _easyDebugMode = easyDebugMode;
    }
}