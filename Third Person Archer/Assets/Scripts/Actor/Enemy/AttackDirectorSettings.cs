using UnityEngine;

/// <summary>
/// Combat rhythm per zone: how many enemies may attack at once and the shortest gap between two attacks starting.
/// Loaded from Resources by <see cref="AttackDirector"/>.
/// </summary>
[CreateAssetMenu(menuName = "Enemies/Attack Director Settings", fileName = AttackDirectorSettings.ResourceName)]
public class AttackDirectorSettings : ScriptableObject
{
    public const string ResourceName = "Attack Director Settings";

    [System.Serializable]
    public struct Zone
    {
        [Min(1)] public int MaxAttackers;
        [Tooltip("Seconds between two attacks starting.")]
        [Min(0f)] public float MinGap;
    }

    [Tooltip("Index 0 = zone 1. Scenes without a zone number use zone 1.")]
    public Zone[] Zones =
    {
        new Zone { MaxAttackers = 1, MinGap = 1.2f },
        new Zone { MaxAttackers = 2, MinGap = 0.8f },
        new Zone { MaxAttackers = 2, MinGap = 0.6f },
    };

    [Tooltip("A granted attack that hasn't started within this time gives its slot back.")]
    [Min(0.1f)] public float StartTimeout = 1.5f;

    [Tooltip("No attack holds a slot for longer than this.")]
    [Min(0.5f)] public float MaxHold = 8f;
}
