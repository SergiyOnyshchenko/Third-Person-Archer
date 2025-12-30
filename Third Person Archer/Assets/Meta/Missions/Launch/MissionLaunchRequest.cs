using Meta.Weapons;
using UnityEngine;

[CreateAssetMenu(fileName = "MissionLaunchRequest", menuName = "GameMeta/MissionLaunchRequest", order = 10)]
public sealed class MissionLaunchRequest : ScriptableObject
{
    [SerializeField] private bool _hasRequest;

    [SerializeField] private MissionType _mode;
    [SerializeField] private MissionData _missionToLoad; // actual mission scene we will load (Campaign/Boss/Sniper or selected campaign for Contracts)
    [SerializeField] private WeaponClass _requiredWeaponClass;

    [SerializeField] private int _zoneIndex;
    [SerializeField] private int _globalCampaignIndex;
    [SerializeField] private int _loopIndex;
    [SerializeField] private int _balanceLoopIndex;

    public bool HasRequest => _hasRequest;

    public MissionType Mode => _mode;
    public MissionData MissionToLoad => _missionToLoad;
    public WeaponClass RequiredWeaponClass => _requiredWeaponClass;

    public int ZoneIndex => _zoneIndex;
    public int GlobalCampaignIndex => _globalCampaignIndex;
    public int LoopIndex => _loopIndex;
    public int BalanceLoopIndex => _balanceLoopIndex;

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
    }

    public void Set(
        MissionType mode,
        MissionData missionToLoad,
        WeaponClass requiredWeaponClass,
        int zoneIndex,
        int globalCampaignIndex,
        int loopIndex,
        int balanceLoopIndex)
    {
        _hasRequest = true;

        _mode = mode;
        _missionToLoad = missionToLoad;
        _requiredWeaponClass = requiredWeaponClass;

        _zoneIndex = zoneIndex;
        _globalCampaignIndex = globalCampaignIndex;
        _loopIndex = loopIndex;
        _balanceLoopIndex = balanceLoopIndex;
    }
}