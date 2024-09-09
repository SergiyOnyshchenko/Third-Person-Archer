using Actor;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(State))]
public class EntitiesAlert : MonoBehaviour
{
    [SerializeField] private GameObject _entitiesContainer;
    [SerializeField] private float _delay;

    [SerializeField, HideInInspector] private Player _player;
    [SerializeField, HideInInspector] private PerceptionInput[] _initers;

    [SerializeField, HideInInspector] private State _state;

    private void Awake()
    {
        _player ??= FindAnyObjectByType<Player>();
        _initers ??= _entitiesContainer.GetComponentsInChildren<PerceptionInput>();
        _state ??= GetComponent<State>();
    }

    private void OnValidate()
    {
        _player = FindAnyObjectByType<Player>();
        _initers = _entitiesContainer.GetComponentsInChildren<PerceptionInput>();
        _state = GetComponent<State>();
    }

    private void OnEnable()
    {
        _state.EnteredState.AddListener(Invoke);
    }

    private void OnDisable()
    {
        _state.EnteredState.RemoveListener(Invoke);
    }

    public void Invoke()
    {
        IEnumerator Async()
        {
            yield return new WaitForSeconds(_delay);

            ITarget playerTarget = null;

            if (_player.TryGetSystem(out Actor.Target target))
                playerTarget = target;

            for (int i = 0; i < _initers.Length; i++)
            {
                _initers[i].ActivatePerception(new ITarget[] { playerTarget });
                _initers[i].ReciveSound("", 1f, _player.gameObject);
            }
        }

        StartCoroutine(Async());
    }
}
