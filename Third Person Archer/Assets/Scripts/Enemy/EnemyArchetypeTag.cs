using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyArchetypeTag : MonoBehaviour
{
    [SerializeField] private EnemyArchetype _archetype = EnemyArchetype.Melee;
    public EnemyArchetype Archetype => _archetype;
}