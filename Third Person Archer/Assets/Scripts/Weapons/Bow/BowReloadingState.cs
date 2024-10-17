using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CustomAnimation;
using CustomAnimation.FPV;
using CustomAnimation.Body;
using DG.Tweening;
using UnityEngine.PlayerLoop;
using Actor;

public class BowReloadingState : ProcessState, IActorIniter
{
    private BowController _bowController;

    public void InitActor(ActorController actor)
    {
        if (actor.TryGetSystem(out BowController bow))
            _bowController = bow;
    }

    public override void Enter()
    {
        base.Enter();

        _bowController.SetReloadSettings();
        _bowController.Reload(FinishProcess);
    }

    public override void Exit()
    {
        _bowController.ResetReloadSettings();

        base.Exit();
    }
}
