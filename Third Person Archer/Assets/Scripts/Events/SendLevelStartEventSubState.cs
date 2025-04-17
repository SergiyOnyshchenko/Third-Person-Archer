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
        
        int level_number = LevelManager.Instance.Database.LevelNumber;
        int level_index = LevelManager.Instance.Database.LevelIndex;
        /*
        SDK_EventSystem.SendLevelStarted(level_number, level_index);
        LevelEventSystem.SendLevelStart();

        if (AppMetricaEventReporter.Instance != null)
            AppMetricaEventReporter.Instance.SendLevelStartEvent(_actor);
        */

        YsoCorp.GameUtils.YCManager.instance.OnGameStarted(level_number);

        base.Exit();
    }
}
