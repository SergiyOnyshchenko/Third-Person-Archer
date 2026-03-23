using System.Collections.Generic;
using UnityEngine;

public class ShootingOutpost : MonoBehaviour
{
    [SerializeField] private PlayerShootingState _shootingState;
    [SerializeField] private List<MonoBehaviour> _indicators = new List<MonoBehaviour>();

    private MainState _previousState;

    private void Awake()
    {
        if (_shootingState == null)
        {
            Debug.LogWarning($"[ShootingOutpost] ShootingState reference is missing on {gameObject.name}");
            return;
        }

        _previousState = _shootingState.GetPreviousState();

        if (_previousState == null)
        {
            Debug.LogWarning($"[ShootingOutpost] No previous state found for {_shootingState.gameObject.name}");
            return;
        }

        _previousState.EnteredState.AddListener(Activate);
        _shootingState.OutOfState.AddListener(Deactivate);
    }

    private void OnDestroy()
    {
        if (_previousState != null)
            _previousState.EnteredState.RemoveListener(Activate);

        if (_shootingState != null)
            _shootingState.OutOfState.RemoveListener(Deactivate);
    }

    private void Activate()
    {
        foreach (var indicator in _indicators)
        {
            if (indicator is IOutpostIndicator outpost)
                outpost.Activate();
        }
    }

    private void Deactivate()
    {
        foreach (var indicator in _indicators)
        {
            if (indicator is IOutpostIndicator outpost)
                outpost.Deactivate();
        }
    }
}