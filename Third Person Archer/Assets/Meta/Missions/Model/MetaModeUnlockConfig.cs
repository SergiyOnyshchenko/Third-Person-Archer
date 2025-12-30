using UnityEngine;

[CreateAssetMenu(fileName = "MetaModeUnlockConfig", menuName = "GameMeta/MetaModeUnlockConfig")]
public class MetaModeUnlockConfig : ScriptableObject
{
    [Header("Company level thresholds (company level = global campaign index + 1)")]
    [Tooltip("Minimum company level required to unlock Contracts mode.")]
    [Min(1)] public int ContractsUnlockCompanyLevel = 5;

    [Tooltip("Minimum company level required to unlock Sniper mode.")]
    [Min(1)] public int SniperUnlockCompanyLevel = 5;
}