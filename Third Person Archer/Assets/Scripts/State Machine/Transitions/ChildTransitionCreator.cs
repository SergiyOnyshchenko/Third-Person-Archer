using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ChildTransitionCreator : MonoBehaviour
{
    [SerializeField] private StateTransition _prefab;
    [SerializeField] private MainState _targetState;
    [Space]
    [SerializeField] private MainState[] _excludeStates;

    private void Awake()
    {
        SetTransitions();
    }

    public void SetTransitions()
    {
        List<MainState> states = GetComponentsInChildren<MainState>().ToList();

        foreach (var state in _excludeStates)
            states.Remove(state);

        foreach (var state in states)
        {
            StateTransition newTransition = Instantiate(_prefab, state.transform);
            newTransition.SetNextStateManualy(_targetState);
        }
    }
}
