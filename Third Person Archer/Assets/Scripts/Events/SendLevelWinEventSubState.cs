using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendLevelWinEventSubState : SubState
{
    public override void Enter()
    {
        base.Enter();

        LevelEventSystem.SendLevelFinish();

        SDK_EventSystem.SendLevelWin(
            LevelManager.Instance.Database.LevelNumber,
            LevelManager.Instance.Database.LevelIndex,
            GameplayTimer.Instance.Timer);
    }  
}
