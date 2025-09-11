using UnityEngine;

[CreateAssetMenu(
    fileName = "EnemyStatsConfig",
    menuName = "Enemy/Enemy Stats Config",
    order = 0)]
public sealed class EnemyStatsConfig : ScriptableObject
{
    [Min(0f), Tooltip("Maximum health of the enemy.")]
    public int Health = 100;

    [Min(0f), Tooltip("Base damage dealt by the enemy.")]
    public int Damage = 10;

    [Min(0), Tooltip("How many coins this enemy drops on death.")]
    public int CoinsDrop = 0;

    [Min(0), Tooltip("How much mana this enemy drops on death.")]
    public int ManaDrop = 0;
}
