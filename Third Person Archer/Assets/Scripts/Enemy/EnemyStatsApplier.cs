using UnityEngine;

[DisallowMultipleComponent]
public sealed class EnemyStatsApplier : MonoBehaviour
{
    [SerializeField] private EnemyStatsConfig _config;
    [Header("Optional: explicitly assign a receiver; otherwise we search on this GameObject")]
    [SerializeField] private MonoBehaviour _receiverComponent;

    private IEnemyStatsReceiver _receiver;

    private void Awake()
    {
        // Resolve receiver dependency (explicit > auto)
        if (_receiverComponent != null)
        {
            _receiver = _receiverComponent as IEnemyStatsReceiver;
            if (_receiver == null)
            {
                Debug.LogError($"Assigned component on {name} does not implement IEnemyStatsReceiver.", this);
            }
        }
        else
        {
            _receiver = GetComponent<IEnemyStatsReceiver>();
            if (_receiver == null)
            {
                Debug.LogError($"No IEnemyStatsReceiver found on {name}. Add a receiver or assign one explicitly.", this);
            }
        }

        if (_config == null)
        {
            Debug.LogError($"Missing EnemyStatsConfig on {name}. Please assign a config.", this);
        }
    }

    private void Start()
    {
        if (_receiver == null || _config == null)
            return;

        Apply(_receiver, _config);
    }

    [ContextMenu("Apply Now (Editor)")]
    private void ApplyNow()
    {
        if (_receiver == null)
        {
            _receiver = GetComponent<IEnemyStatsReceiver>();
        }
        if (_receiver != null && _config != null)
        {
            Apply(_receiver, _config);
        }
    }

    private static void Apply(IEnemyStatsReceiver receiver, EnemyStatsConfig config)
    {
        receiver.SetHealth(config.Health);
        receiver.SetDamage(config.Damage);
        receiver.SetCoinsDrop(config.CoinsDrop);
        receiver.SetManaDrop(config.ManaDrop);
    }
}