using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class StateTransition : MonoBehaviour
{
    [SerializeField] private MainState _nextState;
    [SerializeField] protected bool _isTransit;
    public MainState NextState { get => _nextState; }
    public bool IsTransit { get => _isTransit; }
    public UnityEvent OnTransotion;
    public UnityEvent OnEnter;
    public UnityEvent OnExit;

    public virtual void Enter()
    {
        this.enabled = true;
        _isTransit = false;
        OnEnter?.Invoke();
    }

    public virtual void Exit()
    {
        this.enabled = false;
        OnExit?.Invoke();
    }

    public void SetNextStateManualy(MainState state)
    {
        SetNextState(state);
    }

    protected virtual void DoTransition()
    {
        _isTransit = true;
        OnTransotion?.Invoke();
    }

    protected void SetNextState(MainState state)
    {
        _nextState = state;
    }
}

