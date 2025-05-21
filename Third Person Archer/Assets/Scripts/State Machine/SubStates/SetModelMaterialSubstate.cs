using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class SetModelMaterialSubstate : SubState, IActorIniter
{
    [SerializeField] private Material _material;
    private ModelView _modelView;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out _modelView)) { }
    }

    public override void Enter()
    {
        base.Enter();

        _modelView.SetMaterial(_material);
    }

    public override void Exit()
    {
        _modelView.ResetMaterials();

        base.Exit();
    }
}
