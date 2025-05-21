using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ShowXRaySubstate : SubState, IActorIniter
{
    [SerializeField] private StatePlacement _placement;
    [SerializeField] private bool _value = true;
    [Space]
    [SerializeField] private bool _isPermanent = true;
    private XRayController _xRay;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _xRay)) { }
    }

    public override void Enter()
    {
        base.Enter();

        if (_xRay == null)
            return;

        if (_placement == StatePlacement.OnEnter)
        {
            _xRay.Show(_value);
        }
    }

    public override void Exit()
    {
        if (_xRay == null)
            return;

        if (_placement == StatePlacement.OnExit)
        {
            _xRay.Show(_value);
        }
        else if (_placement == StatePlacement.OnEnter && !_isPermanent)
        {
            _xRay.Show(!_value);
        }

        base.Exit();
    }
}
