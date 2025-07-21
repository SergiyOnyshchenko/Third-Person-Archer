using System.Collections;
using System.Collections.Generic;
using Actor;
using UnityEngine;

public class SendLevelStartEventSubState : SubState, IActorIniter
{
    private ActorController _actor;
    public void InitActor(ActorController actor)
    {
        _actor = actor;
    }

    public override void Exit()
    {
        LevelEventSystem.SendLevelStart();

        int level_number = LevelManager.Instance.CurrentMission.ID;
        YsoCorp.GameUtils.YCManager.instance.OnGameStarted(level_number);

        base.Exit();
    }
}
