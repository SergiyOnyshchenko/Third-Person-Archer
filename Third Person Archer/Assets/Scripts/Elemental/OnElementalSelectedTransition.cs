using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class OnElementalSelectedTransition : StateTransition, IActorIniter
{
    private ElementalController _elementalController;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out ElementalController elementalController))
            _elementalController = elementalController;
    }

    public override void Enter()
    {
        base.Enter();
        _elementalController.OnElementSelected.AddListener(DoTransition);
    }

    public override void Exit() 
    {
        _elementalController.OnElementSelected.RemoveListener(DoTransition);
        base.Exit();
    }
}
