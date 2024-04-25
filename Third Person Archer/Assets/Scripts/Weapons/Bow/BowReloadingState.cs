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

    private void FixedUpdate()
    {
        /*
        var pose = _animatorController.LerpPoses(_poses[_index], _poses[_index + 1], _value - (float)_index);
        _animatorController.DoPose(pose);
        */
    }

    
}
