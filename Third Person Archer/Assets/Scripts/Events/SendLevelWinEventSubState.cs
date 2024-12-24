using System.Collections;
using System.Collections.Generic;
using Actor;
using Actor.Properties;
using MoreMountains.Tools;
using UnityEngine;

public class SendLevelWinEventSubState : SubState, IActorIniter
{
    private ActorController _actor;
    public void InitActor(ActorController actor)
    {
        _actor = actor;
    }

    public override void Enter()
    {
        base.Enter();

        int level_number = LevelManager.Instance.Database.LevelNumber;
        int level_index = LevelManager.Instance.Database.LevelIndex;
        float timer = GameplayTimer.Instance.Timer;

        LevelEventSystem.SendLevelFinish();

        SDK_EventSystem.SendLevelWin(
            level_number,
            level_index,
            timer);


        if (AppMetricaEventReporter.Instance != null)
            AppMetricaEventReporter.Instance.SendLevelWinEvent(_actor);
    }  
}
