using System.Collections;
using System.Collections.Generic;
using Actor;
using DG.Tweening;
using UnityEngine;
using Input = UnityEngine.Input;

public class SpearThrowState : ProcessState, IActorIniter
{
    private SpearController _spearController;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out SpearController spear))
            _spearController = spear;
    }

    public override void Enter()
    {
        base.Enter();

        _spearController.SetStartSettings();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            _spearController.BeginPull();
        }

        if (Input.GetMouseButton(0))
        {
            _spearController.HoldPull();
        }

        if (Input.GetMouseButtonUp(0))
        {
            _spearController.ReleasePull();
            DOVirtual.DelayedCall(0.5f, FinishProcess);
        }
    }
}
