using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateMashine : MonoBehaviour
{
    [SerializeField] private MainState _startState;
    [SerializeField] private MainState _currentState;

    private void Start() 
    {
        Reset();
    }

    private void Update()
    {
        if (_currentState == null)
            return;

        MainState nextState = _currentState.GetNextState();

        if (nextState != null)
            Transit(nextState);
    }

    private void Reset()
    {
        _currentState = _startState;

        if (_currentState != null)
            _currentState.Enter();
    }

    private void Transit(MainState nextState)
    {
        if (_currentState != null)
            _currentState.Exit();

        _currentState = nextState;

        if (_currentState != null)
            _currentState.Enter();
    }
}

