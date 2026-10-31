using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainState : State
{
    [Header("States")]
    [SerializeField] private List<StateTransition> _transitions = new List<StateTransition>();
    [SerializeField] private List<SubState> _subStates = new List<SubState>();
    public List<StateTransition> Transitions { get => _transitions;}

    private bool _childrenEntered;

    /// <summary>False: the state holds its transitions and substates back until it calls <see cref="EnterChildren"/>.</summary>
    protected virtual bool EnterChildrenOnEnter => true;

    protected override void Awake()
    {
        base.Awake();

        InitTransitions();
        InitSubstates();
    }

    public override void Enter()
    {
        base.Enter();

        if (EnterChildrenOnEnter)
            EnterChildren();
    }

    public override void Exit()
    {
        if (_childrenEntered)
        {
            foreach (SubState subState in _subStates)
                subState.Exit();

            foreach (StateTransition transition in _transitions)
                transition.Exit();

            _childrenEntered = false;
        }

        base.Exit();
    }

    protected void EnterChildren()
    {
        if (_childrenEntered)
            return;

        _childrenEntered = true;

        foreach (StateTransition transition in _transitions)
            transition.Enter();

        foreach (SubState subState in _subStates)
            subState.Enter();
    }

    public MainState GetNextState()
    {
        // Transitions that haven't entered yet still hold last time's result.
        if (_transitions == null || !_childrenEntered)
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
