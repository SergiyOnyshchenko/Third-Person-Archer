using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainState : State
{
    [SerializeField] private List<StateTransition> _transitions = new List<StateTransition>();
    private List<SubState> _subStates = new List<SubState>();
    public List<StateTransition> Transitions { get => _transitions;}

    protected override void Awake()
    {
        base.Awake();

        InitTransitions();
        InitSubstates();
    }

    public override void Enter()
    {
        base.Enter();

        foreach (StateTransition transition in _transitions)
            transition.Enter();

        foreach (SubState subState in _subStates)
            subState.Enter();
    }

    public override void Exit()
    {
        foreach (SubState subState in _subStates)
            subState.Exit();

        foreach (StateTransition transition in _transitions)
            transition.Exit();
        
        base.Exit();
    }

    public MainState GetNextState()
    {
        if (_transitions == null)
            return null;

        foreach (StateTransition transition in _transitions)
        {
            if (transition.IsTransit)
                return transition.NextState;
        }

        return null;
    }

    public void AddTransition(StateTransition newTransition)
    {
        foreach (var transition in _transitions)
        {
            if (transition == newTransition)
                return;
        }

        _transitions.Add(newTransition);
    }

    private void InitTransitions()
    {
        var childTransitions = GetComponentsInChildren<StateTransition>();

        foreach (var transition in childTransitions)
            AddTransition(transition);
    }

    private void InitSubstates()
    {
        var childSubstates = GetComponentsInChildren<SubState>();

        foreach (var substate in childSubstates)
            AddSubstate(substate);
    }

    private void AddSubstate(SubState newState)
    {
        foreach (var state in _subStates)
        {
            if (state == newState)
                return;
        }

        _subStates.Add(newState);
    }
}
