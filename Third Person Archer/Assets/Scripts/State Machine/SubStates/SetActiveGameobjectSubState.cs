using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetActiveGameobjectSubState : SubState
{
    [SerializeField] private GameObject _target;
    [SerializeField] private bool _value;
    [SerializeField] private StatePlacement _placement;

    public override void Enter()
    {
        base.Enter();

        if (_placement == StatePlacement.OnEnter)
            _target.SetActive(_value);
    }

    public override void Exit()
    {
        if (_placement == StatePlacement.OnExit)
            _target.SetActive(_value);

        base.Exit();
    }
}
