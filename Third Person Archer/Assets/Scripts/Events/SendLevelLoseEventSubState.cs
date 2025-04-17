using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Actor;
using UnityEngine;

public class SendLevelLoseEventSubState : SubState, IActorIniter
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
        float progress = EnemyManager.Instance.GetDeadEnemiesRatio();

        /*
        LevelEventSystem.SendLevelFinish();

        SDK_EventSystem.SendLevelLose(
            level_number,
            level_index,
            progress,
            timer);

        if (AppMetricaEventReporter.Instance != null)
            AppMetricaEventReporter.Instance.SendLevelLostEvent(_actor);
        */

        YsoCorp.GameUtils.YCManager.instance.OnGameFinished(false);
    }
}
