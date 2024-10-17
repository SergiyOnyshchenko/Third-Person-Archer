using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendLevelLoseEventSubState : SubState
{
    public override void Enter()
    {
        base.Enter();

        LevelEventSystem.SendLevelFinish();

        SDK_EventSystem.SendLevelLose(
            LevelManager.Instance.Database.LevelNumber,
            LevelManager.Instance.Database.LevelIndex,
            EnemyManager.Instance.GetDeadEnemiesRatio(),
            GameplayTimer.Instance.Timer);
    }
}
