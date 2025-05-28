using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;

public class OnInputActiveTransition : StateTransition
{
    [SerializeField] private Actor.Input _input;
    [SerializeField] private bool _isActive = true;

    private void Update()
    {
        if(_input.IsActive == _isActive)
            DoTransition(); 
    }
}
