using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor.Properties;


public class SetBooleanProperySubstate : SubState
{
    [SerializeField] private BooleanProperty _property;
    [SerializeField] private bool _value;
    [SerializeField] private StatePlacement _placement;

    public override void Enter()
    {
        base.Enter();

        if(_placement == StatePlacement.OnEnter)
            _property.SetValue(_value);
    }

    public override void Exit() 
    {
        if (_placement == StatePlacement.OnExit)
            _property.SetValue(_value);

        base.Exit();
    }
}
