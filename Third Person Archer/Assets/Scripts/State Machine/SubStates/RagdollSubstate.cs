using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Actor;

public class RagdollSubstate : SubState, IActorIniter
{
    [SerializeField] private StatePlacement _placement;
    [SerializeField] private float _delay;

    private RagdollControll _ragdollControll;

    public void InitActor(ActorController actor)
    {
        if(actor.TryGetSystem(out _ragdollControll)) { }
    }

    public override void Enter()
    {
        base.Enter();

        if (_placement == StatePlacement.OnEnter)
            DOVirtual.DelayedCall(_delay, () => _ragdollControll.MakePhysical());
    }

    public override void Exit()
    {
        if (_placement == StatePlacement.OnExit)
            DOVirtual.DelayedCall(_delay, () => _ragdollControll.MakePhysical());

        base.Exit();
    }
}
