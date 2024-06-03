using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelFlowTransitionSetter : MonoBehaviour
{
    [SerializeField] private MainState[] _states;

    private void Awake()
    {
        SetTransitions();
    }

    [ContextMenu("Set Transitions")]
    public void SetTransitions()
    {
        for (int i = 0; i < _states.Length - 1; i++)
        {
            StateTransition transition = _states[i].GetComponentInChildren<StateTransition>();
            transition.SetNextStateManualy(_states[i + 1]);
        }
    }
}
