using System;
using UnityEngine;

public partial class BalanceConfig
{
    /// <summary>
    /// Replay of a cleared path level (MissionType.Contracts): the level's own class, no path progress,
    /// gold + essence by the level's zone, no upward auto-scaling of enemies.
    /// </summary>
    [Serializable]
    public sealed class ReplayModule
    {
        [Header("Enemies")]
        [Tooltip("Enemy damage multiplier vs the campaign formula (the formula itself is disabled for now).")]
        [Min(0f)] public float EnemyDamageMultiplier = 0.6f;

        [Tooltip("Enemies never need more than this many body shots of the played class.")]
        [Min(1)] public int MaxBodyShots = 2;
    }
}
