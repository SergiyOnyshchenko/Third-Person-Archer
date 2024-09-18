using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ChangeModelEmmissionSubstate : SubState, IActorIniter
{
    [SerializeField] private Color _color;
    private ModelView _modelView;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out ModelView view))
            _modelView = view;
    }

    public override void Enter()
    {
        base.Enter();
        _modelView.SetEmmissionColor(_color);
    }
}
