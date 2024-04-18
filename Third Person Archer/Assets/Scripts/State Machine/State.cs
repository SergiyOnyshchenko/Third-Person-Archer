using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class State : MonoBehaviour
{
    public UnityEvent EnteredState;
    public UnityEvent OutOfState;

    protected virtual void Awake() 
    {
        this.enabled = false;
    }
    
    public virtual void Enter()
    {
        this.enabled = true;
        EnteredState?.Invoke();
    }

    public virtual void Exit()
    {
        OutOfState?.Invoke();
        this.enabled = false;
    }
    
}

