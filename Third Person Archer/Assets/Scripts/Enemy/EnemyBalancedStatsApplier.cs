using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyBalancedStatsApplier : MonoBehaviour
{
    [SerializeField] private EnemyArchetypeTag _archetypeTag;

    private IEnemyStatsReceiver _receiver;

    private void Awake()
    {
        if (_archetypeTag == null)
            _archetypeTag = GetComponent<EnemyArchetypeTag>();

        _receiver = GetComponentInChildren<IEnemyStatsReceiver>(true);
    }

    private void Start()
    {
        Apply();
    }

    public void Apply()
    {
        var rt = GameplayRuntime.Instance;
        if (rt == null)
            return;

        var ctx = rt.Context;
        if (ctx == null || !ctx.IsValid)
            return;

        if (_receiver == null || _archetypeTag == null)
            return;

        var balance = rt.Balance;
        if (balance == null)
            return;

        var stats = balance.GetEnemyStats(
            ctx,
            _archetypeTag.Archetype,
            rt.ContractsCompletedIndex,
            rt.SniperCompletedIndex,
            rt.IsMultiplayer);

        _receiver.SetHealth(stats.MaxHp);
        _receiver.SetDamage(10);
        //_receiver.SetDamage(stats.Damage);
    }
}