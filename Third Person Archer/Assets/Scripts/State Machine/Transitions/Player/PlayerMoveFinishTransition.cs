using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actor;
using DG.Tweening;

public class PlayerMoveFinishTransition : StateTransition, IActorIniter
{
    private Mover _mover;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out Mover mover))
            _mover = mover;
    }

    public override void Enter()
    {
        base.Enter();

        //DOVirtual.DelayedCall(0.1f, () => _mover.OnMovingFinished.AddListener(DoTransition));

        _mover.OnMovingFinished.AddListener(DoTransition);
    }

    public override void Exit()
    {
        _mover.OnMovingFinished.RemoveListener(DoTransition);

        base.Exit();
    }
}
