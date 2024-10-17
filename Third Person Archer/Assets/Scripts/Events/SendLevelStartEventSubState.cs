using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendLevelStartEventSubState : SubState
{
    public override void Exit()
    {
        SDK_EventSystem.SendLevelStarted(LevelManager.Instance.Database.LevelNumber, LevelManager.Instance.Database.LevelIndex);
        LevelEventSystem.SendLevelStart();

        base.Exit();
    }
}
