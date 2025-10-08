using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class ProjectileMoveTypeTransition : StateTransition, IActorIniter
{
    [SerializeField] private ProjectileMoveType _type;
    private ProjectileMoveTypeProperty _property;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetProperty(out _property)) { }
    }

    public override void Enter()
    {
        base.Enter();

        if (_property == null)
            return;

        CheckTransition();
        _property.OnPropertyChanged += CheckTransition;
    }

    public override void Exit()
    {
        if (_property != null)
            _property.OnPropertyChanged -= CheckTransition;
            
        base.Exit();
    }

    private void CheckTransition()
    {
        if (_property.Value == _type)
            DoTransition();
    }
}