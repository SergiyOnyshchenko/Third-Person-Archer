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
            if (_states[i] == null)
                continue;

            StateTransition transition = _states[i].GetComponentInChildren<StateTransition>();

            if(transition != null)
                transition.SetNextStateManualy(_states[i + 1]);
        }
    }
}
