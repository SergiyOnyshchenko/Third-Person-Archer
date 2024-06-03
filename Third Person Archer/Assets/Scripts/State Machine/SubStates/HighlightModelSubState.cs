using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class HighlightModelSubState : SubState, IActorIniter
{
    [SerializeField] private StatePlacement _placement;
    [SerializeField] private bool _isPermanent = true;
    private ModelView _modelView;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out ModelView view))
            _modelView = view;
    }

    public override void Enter()
    {
        base.Enter();

        if (_modelView == null)
            return;

        if (_placement == StatePlacement.OnEnter)
        {
            _modelView.Highlight(true);
        }
    }

    public override void Exit() 
    {
        if (_modelView == null)
            return;

        if (_placement == StatePlacement.OnExit)
        {
            _modelView.Highlight(true);
        }
        else if (_placement == StatePlacement.OnEnter && !_isPermanent)
        {
            _modelView.Highlight(false);
        }

        base.Exit();
    }
}
