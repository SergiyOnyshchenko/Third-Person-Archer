using Meta.Weapons;
using UnityEngine;

[CreateAssetMenu(fileName = "Mission", menuName = "GameMeta/MissionData", order = 1)]
public class MissionData : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Stable unique id. Used for saving and contract eligibility tracking. Do not change once shipped.")]
    [SerializeField] private string _id;

    [Header("UI Metadata")]
    [Tooltip("Name shown in UI.")]
    [SerializeField] private string _displayName;

    [Tooltip("Optional short description for UI.")]
    [TextArea(2, 5)]
    [SerializeField] private string _description;

    [Header("Type & Scene")]
    [Tooltip("Mission type this asset represents.")]
    [SerializeField] private MissionType _missionType;

    [Tooltip("Scene to load for this mission.")]
    [SerializeField] private SceneReference _scene;

    [Header("Weapon Requirement")]
    [Tooltip("Weapon class required by this mission.")]
    [SerializeField] private WeaponClass _baseWeaponClass = WeaponClass.Bow;

    [Tooltip("If true, this mission uses a fixed weapon class (overrides the base class). Recommended for Sniper missions.")]
    [SerializeField] private bool _useFixedWeaponClass = false;

    [Tooltip("Fixed weapon class when Use Fixed Weapon Class is enabled.")]
    [SerializeField] private WeaponClass _fixedWeaponClass = WeaponClass.Bow;

    [Header("Reward Override (Optional)")]
    [Tooltip("If enabled, this mission uses override values instead of the BalanceConfig reward profile.")]
    [SerializeField] private bool _useRewardOverride = false;

    [Tooltip("Cash reward when override is enabled.")]
    [Min(0)]
    [SerializeField] private int _overrideBaseCash = 0;

    [Tooltip("Token amount per token type when override is enabled (for Contracts/Sniper/Boss style rewards).")]
    [Min(0)]
    [SerializeField] private int _overrideBaseTokensPerType = 0;

    public string ID => _id;
    public string Name => _displayName;
    public string Description => _description;

    public MissionType MissionType => _missionType;
    public SceneReference Scene => _scene;

    public WeaponClass BaseWeaponClass => _baseWeaponClass;
    public bool UseFixedWeaponClass => _useFixedWeaponClass;
    public WeaponClass FixedWeaponClass => _fixedWeaponClass;

    public bool UseRewardOverride => _useRewardOverride;
    public int OverrideBaseCash => _overrideBaseCash;
    public int OverrideBaseTokensPerType => _overrideBaseTokensPerType;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (string.IsNullOrWhiteSpace(_id))
            _id = System.Guid.NewGuid().ToString("N");

        if (string.IsNullOrWhiteSpace(_displayName))
            _displayName = name;

        // If mission is Sniper, default to fixed weapon class unless you override.
        if (_missionType == MissionType.Sniper)
        {
            if (!_useFixedWeaponClass)
                _useFixedWeaponClass = true;
        }
    }
#endif
}